namespace MiniApi.Models;

public class User
{
    public Guid Id { get;set; }
    public string Nombre { get; set; } =string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
    public int IntentosFallidos { get; set;}

    //  El constructor vacío que necesita obligatoriamente la base de datos
    public User()
    {
    }

}