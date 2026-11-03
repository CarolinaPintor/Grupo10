using System.ComponentModel.DataAnnotations;
using MiniApi.Common;
using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterUserRequest request)
    {
        // Normalizo el email para que "A@x.com" y "a@x.com" sean el mismo usuario
        request = request with { Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty };

        // Validar que el email tenga un formato básico correcto
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            // Corrección: Usamos una List<string> y la constante ErrorCodes.USR_002
            var errores = new List<string> { "El formato del correo electrónico es inválido." };
            throw new MiniApi.Exceptions.ValidationException(ErrorCodes.USR_002, errores);
        }

        //  Validar formato de datos (USR-002)
        List<ValidationResult> resultadosValidacion = new List<ValidationResult>();
        ValidationContext contextoValidacion = new ValidationContext(request);
        bool esValido = Validator.TryValidateObject(request, contextoValidacion, resultadosValidacion, true);

        if (!esValido)
        {
            IEnumerable<string> errores = resultadosValidacion.Select(r => r.ErrorMessage ?? "Dato inválido");
            throw new MiniApi.Exceptions.ValidationException(ErrorCodes.USR_002, errores);
        }

        // Validar la regla de email duplicado (USR-001)
        User? usuarioExistente = await _userRepository.GetByEmailAsync(request.Email);
        if (usuarioExistente != null)
        {
            throw new BusinessRuleException(ErrorCodes.USR_001, "El email ya está registrado.");
        }

        // Hashear la contraseña 
        string contrasenaHasheada = PasswordHasher.Hash(request.Password);

        // Creo la entidad User 
        User nuevoUsuario = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = contrasenaHasheada,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        // Guardar el nuevo usuario en la base de datos
        await _userRepository.CreateAsync(nuevoUsuario);

        // Retornar el DTO seguro (sin la contraseña)
        return new RegisterResponse(
            nuevoUsuario.Id,
            nuevoUsuario.Nombre,
            nuevoUsuario.Apellido,
            nuevoUsuario.Email,
            nuevoUsuario.FechaRegistro,
            nuevoUsuario.Activo
        );
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Normalizo el email igual que en el registro
        request = request with { Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty };

        // USR-002: email y contraseña son obligatorios
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new MiniApi.Exceptions.ValidationException(ErrorCodes.USR_002, new List<string> { "El email y la contraseña son obligatorios." });
        }

        // 1. Buscar al usuario en la base de datos
        User? usuario = await _userRepository.GetByEmailAsync(request.Email);

        // USR-003: Si el email no existe, devolvemos error genérico.
        if (usuario == null)
        {
            throw new UnauthorizedException(ErrorCodes.USR_003, "Credenciales incorrectas.");
        }

        // Evaluar estados de bloqueo antes de revisar la contraseña
        if (!usuario.Activo)
        {
            // Si está inactivo y tiene 3 o más intentos, es por bloqueo automático (USR-004)
            if (usuario.IntentosFallidos >= 3)
            {
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
            }
            // Si está inactivo pero NO llegó a 3 intentos, es una suspensión manual (USR-005)
            else
            {
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
            }
        }

        // 2. Verificar la contraseña de forma segura
        bool esContrasenaValida = PasswordHasher.Verify(request.Password, usuario.PasswordHash);

        if (!esContrasenaValida)
        {
            // 3. Sumar un intento fallido y guardar en BD
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= 3)
            {
                usuario.Activo = false; // Bloquear la cuenta
                await _userRepository.UpdateAsync(usuario);
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
            }

            await _userRepository.UpdateAsync(usuario);
            throw new UnauthorizedException(ErrorCodes.USR_003, "Credenciales incorrectas.");
        }

        // 4. Contraseña correcta: Resetear intentos si venía equivocándose
        if (usuario.IntentosFallidos > 0)
        {
            usuario.IntentosFallidos = 0;
            await _userRepository.UpdateAsync(usuario);
        }

        // 5. Retornar el DTO exitoso (sin la contraseña)
        return new LoginResponse(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email);
    }
}