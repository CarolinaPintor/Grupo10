using System.ComponentModel.DataAnnotations;

namespace MiniApi.DTOs;

// Request para la creación de una nueva orden (POST /api/orders).
// Contiene el ID del usuario que realiza la compra y la lista de ítems.
public record CreateOrderRequest(
    [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
    Guid UsuarioId,

    [Required(ErrorMessage = "La lista de ítems es obligatoria.")]
    [MinLength(1, ErrorMessage = "La orden debe contener al menos un ítem.")]
    List<CreateOrderItemRequest> Items
);

// Representa un ítem individual dentro de la solicitud de la orden.
public record CreateOrderItemRequest(
    [Required(ErrorMessage = "El ID del producto es obligatorio.")]
    Guid ProductoId,

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    int Cantidad
);

// Respuesta devuelta tras la creación exitosa de una orden (201 Created).
public record OrderResponse(
    Guid Id,
    Guid UsuarioId,
    List<OrderItemResponse> Items,
    decimal Total,
    string Estado,
    DateTime FechaCreacion
);

// Detalle del ítem devuelto en la respuesta de la orden.
public record OrderItemResponse(
    Guid ProductoId,
    int Cantidad,
    decimal PrecioUnitario
);