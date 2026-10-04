public clss User
{
    public Guid Id;
public string Nombre;
public string Apellido;
public string Email;
public string PasswordHash;
public DateTime FechaRegistro;
public bool Activo;
public int IntentosFallidos;
public User(guid Id, string Nombre, string Apellido, string Email, string PasswordHash, DateTime FechaRegistro, bool Activo, int IntentosFallidos)
{
    this.Id = Id;
    this.Nombre = Nombre;
    this.Apellido = Apellido;
    this.Email = Email;
    this.PasswordHash = PasswordHash;
    this.FechaRegistro = FechaRegistro;
    this.Activo = Activo;
    this.IntentosFallidos = IntentosFallidos;
}
}