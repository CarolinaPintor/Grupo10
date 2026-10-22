using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class CartEndpointsExtension
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cart")
                       .WithTags("Cart");

        // GET: obtener el carrito de un usuario
        group.MapGet("/{userId:guid}", async (
            Guid userId,
            CartService service) =>
        {
            var cart = await service.GetAsync(userId);
            return Results.Ok(cart);
        })
        .WithName("GetCart")
        .WithSummary("Obtener el carrito de un usuario")
        .WithDescription(
            "Devuelve los items del carrito del usuario indicado. " +
            "Si el usuario no tiene carrito responde CRT-001.")
        .Produces<CartResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
