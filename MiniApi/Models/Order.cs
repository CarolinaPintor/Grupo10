public class Order
{
    public Guid Id;

    public Guid UsuarioID;
    public OrderItem[] Items;
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
        Items = items?.ToList() ?? new List<OrderItem>();
        Total = Items.Sum(item => item.Cantidad * item.PrecioUnitario);
        Estado = "Pendiente";
        FechaCreacion = DateTime.UtcNow;
    }
}