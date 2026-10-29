public class Order
{
    public Guid Id;

    public Guid UsuarioId;
    public OrderItem[] Items = Array.Empty<OrderItem>();
    public decimal Total;
    public string Estado;
    public DateTime FechaCreacion;

    public Order()
    {
        Id = Guid.NewGuid();
        Estado = "Pendiente";
        FechaCreacion = DateTime.UtcNow;
    }

    public Order(Guid usuarioId, IEnumerable<OrderItem> items)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Items = items.ToArray();
        Total = Items.Sum(item => item.Cantidad * item.PrecioUnitario);
        Estado = "Pendiente";
        FechaCreacion = DateTime.UtcNow;
    }
}