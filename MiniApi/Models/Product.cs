namespace Products.API.Models;

public class Product
{
    public Guid Id;
    public string Nombre;
    public string Descripcion;
    public decimal Precio;
    public int Stock;
    public string Categoria;
    public DateTime FechaCreacion;

   public Product(guid Id, string Nombre, string Descripcion, decimal Precio, int Stock, string Categoria, DateTime FechaCreacion)
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