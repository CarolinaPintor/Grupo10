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

        // Paso los ítems a un arreglo y voy sumando el total con un acumulador
        var listaItems = new List<OrderItem>();
        decimal total = 0;
        foreach (OrderItem item in items)
        {
            listaItems.Add(item);
            total += item.Cantidad * item.PrecioUnitario;
        }

        Items = listaItems.ToArray();
        Total = total;
        Estado = "Pendiente";
        FechaCreacion = DateTime.UtcNow;
    }
}