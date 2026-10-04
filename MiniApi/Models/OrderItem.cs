public class OrderItem
{
    public Guid ProductoId;
    public int Cantidad;
    public decimal PrecioUnitario;

    public OrderItem(Guid ProductoId, int Cantidad, decimal PrecioUnitario)
    {
        this.ProductoId = ProductoId;
        this.Cantidad = Cantidad;
        this.PrecioUnitario = PrecioUnitario;
    }
}