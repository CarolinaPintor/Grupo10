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
public User(Guid Id, string Nombre, string Apellido, string Email, string PasswordHash, DateTime FechaRegistro, bool Activo, int IntentosFallidos)
{
    this.Id = Id;
    this.Nombre = Nombre;
    this.Apellido = Apellido;
    this.Email = Email;
    this.PasswordHash = PasswordHash;
    this.FechaRegistro = FechaRegistro;
    this.Activo = true;
    this.IntentosFallidos = 0;
}
}