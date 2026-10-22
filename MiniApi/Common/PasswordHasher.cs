using System.Security.Cryptography;

namespace MiniApi.Common;

public static class PasswordHasher
{
    // El "Salt" (sal) es un texto aleatorio que se le suma a la contraseña.
    // Sirve para que si dos usuarios tienen la clave "123456", en la BD se vean completamente distintas.
    private const int SaltSize = 16;

    private const int KeySize = 32;

    // Hacemos que el proceso se repita 100.000 veces. 
    // Para el usuario toma medio segundo, pero al hacker le hace imposible un ataque de fuerza bruta.
    private const int Iterations = 100_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// Recibe la contraseña normal, le agrega el Salt, la procesa 100.000 veces y devuelve el texto incomprensible (Hash).

    public static string Hash(string password)
    {
        // 1. Generamos un Salt verdaderamente aleatorio y seguro
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // 2. Mezclamos la contraseña con el Salt 100.000 veces usando el algoritmo nativo PBKDF2
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // 3. Guardamos todo junto separado por puntos: Iteraciones.Sal.HashFinal
        // Así, cuando el usuario vuelva a entrar, el sistema sabe exactamente qué Sal e iteraciones usar para comparar.
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    // Revisa si la contraseña que escribió el usuario al loguearse coincide con el texto raro guardado en la BD.
    public static bool Verify(string password, string hashedPassword)
    {
        // Separamos el string guardado para recuperar las iteraciones, el Salt y el Hash original
        var parts = hashedPassword.Split('.');
        if (parts.Length != 3) return false;

        try
        {
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            // Volvemos a procesar la contraseña que acaba de escribir el usuario,
            // usando EXACTAMENTE la misma Sal y cantidad de iteraciones de la primera vez.
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                Algorithm,
                KeySize);

            /* Usamos FixedTimeEquals en vez de un "==" tradicional.
            Esto obliga al sistema a tardar exactamente el mismo tiempo en comparar la cadena completa, 
            sin importar si falló en la primera letra o en la última. Así evitamos "Timing Attacks". */

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            // Si el formato en la BD está roto o fue manipulado, fallamos de manera segura
            return false;
        }
    }
}
