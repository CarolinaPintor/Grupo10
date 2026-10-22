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

        // POST: agregar un producto al carrito
        group.MapPost("/{userId:guid}/items", async (
            Guid userId,
            AddCartItemRequest request,
            CartService service) =>
        {
            var cart = await service.AddItemAsync(userId, request);
            return Results.Ok(cart);
        })
        .WithName("AddCartItem")
        .WithSummary("Agregar un producto al carrito")
        .WithDescription(
            "Agrega el producto al carrito (lo crea si no existe). Si el producto ya estaba, " +
            "suma la cantidad. Valida cantidad (CRT-004), producto (CRT-002) y stock (CRT-003).")
        .Produces<CartResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT: cambiar la cantidad de un producto del carrito
        group.MapPut("/{userId:guid}/items/{productId:guid}", async (
            Guid userId,
            Guid productId,
            UpdateCartItemRequest request,
            CartService service) =>
        {
            var cart = await service.UpdateItemAsync(userId, productId, request);
            return Results.Ok(cart);
        })
        .WithName("UpdateCartItem")
        .WithSummary("Actualizar la cantidad de un item del carrito")
        .WithDescription(
            "Reemplaza la cantidad del producto en el carrito. Valida cantidad (CRT-004), " +
            "carrito (CRT-001), producto (CRT-002) y stock (CRT-003).")
        .Produces<CartResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
