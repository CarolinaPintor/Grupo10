public class OrderItem
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public OrderItem(Guid ProductoId, int Cantidad, decimal PrecioUnitario)
    {
        this.ProductoId = ProductoId;
        this.Cantidad = Cantidad;
        this.PrecioUnitario = PrecioUnitario;
    }
}