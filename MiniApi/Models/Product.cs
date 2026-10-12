namespace Products.API.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; } //de esta forma Descripcion puede ser null
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; } = string.Empty; //de esta forma Categoria no puede ser null
    public DateTime FechaCreacion { get; set; }

   public Product(Guid Id, string Nombre, string Descripcion, decimal Precio, int Stock, string Categoria, DateTime FechaCreacion)
    {
        this.Id = Id;
        this.Nombre = Nombre;
        this.Descripcion = Descripcion;
        this.Precio = Precio;
        this.Stock = Stock;
        this.Categoria = Categoria;
        this.FechaCreacion = DateTime.UtcNow;
    }



}