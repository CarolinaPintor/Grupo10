using System.ComponentModel.DataAnnotations;

namespace MiniApi.DTOs;

// Request para la creación de una nueva orden (POST /api/orders).
// Contiene el ID del usuario que realiza la compra y la lista de ítems.
public record CreateOrderRequest(
    [property: Required(ErrorMessage = "El ID del usuario es obligatorio.")]
    Guid UsuarioId,

    [property: Required(ErrorMessage = "La lista de ítems es obligatoria.")]
    [property: MinLength(1, ErrorMessage = "La orden debe contener al menos un ítem.")]
    List<CreateOrderItemRequest> Items
);

// Representa un ítem individual dentro de la solicitud de la orden.
public record CreateOrderItemRequest(
    [property: Required(ErrorMessage = "El ID del producto es obligatorio.")]
    Guid ProductoId,

    [property: Required(ErrorMessage = "La cantidad es obligatoria.")]
    [property: Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    int Cantidad
);
// Request para actualizar el estado de una orden (PUT /api/orders/{id}/status)
public record UpdateOrderStatusRequest(
    [property: Required(ErrorMessage = "El estado es obligatorio.")]
    string Estado
);

// Respuesta de PUT /api/orders/{id}/status (200 OK): solo id, nuevo estado y fecha del cambio.
public record UpdateOrderStatusResponse(
    Guid Id,
    string Estado,
    DateTime FechaActualizacion
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