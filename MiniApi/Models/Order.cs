namespace MiniApi.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public DateTime FechaCreacion { get; set; }

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