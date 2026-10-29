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

        // DELETE: quitar un producto del carrito
        group.MapDelete("/{userId:guid}/items/{productId:guid}", async (
            Guid userId,
            Guid productId,
            CartService service) =>
        {
            await service.RemoveItemAsync(userId, productId);
            return Results.NoContent();
        })
        .WithName("RemoveCartItem")
        .WithSummary("Quitar un producto del carrito")
        .WithDescription(
            "Elimina el producto del carrito. Responde CRT-001 si el usuario no tiene carrito " +
            "y CRT-002 si el producto no estaba en el carrito.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE: vaciar el carrito completo
        group.MapDelete("/{userId:guid}", async (
            Guid userId,
            CartService service) =>
        {
            await service.ClearAsync(userId);
            return Results.NoContent();
        })
        .WithName("ClearCart")
        .WithSummary("Vaciar el carrito completo")
        .WithDescription(
            "Elimina el carrito del usuario con todos sus items. " +
            "Responde CRT-001 si el usuario no tiene carrito.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
