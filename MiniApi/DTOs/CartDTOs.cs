namespace MiniApi.DTOs;

// Request para agregar un producto al carrito (POST /api/cart/{userId}/items).
// La cantidad no se valida acá: una cantidad <= 0 se informa desde el servicio con CRT-004.
public record AddCartItemRequest(Guid ProductoId, int Cantidad);

// Request para cambiar la cantidad de un item (PUT /api/cart/{userId}/items/{productId}).
public record UpdateCartItemRequest(int Cantidad);

// Item del carrito en las respuestas.
public record CartItemResponse(Guid ProductoId, int Cantidad);

// Carrito en las respuestas. El modelo Cart usa campos públicos y no se serializa a JSON.
public record CartResponse(Guid UsuarioId, List<CartItemResponse> Items, DateTime FechaActualizacion);
