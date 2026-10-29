using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class OrderEndpointsExtension
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
                       .WithTags("Orders");

        // GET: Listar órdenes con filtro opcional por usuarioId
        group.MapGet("/", async (
            OrderService service,
            Guid? usuarioId) =>
        {
            var orders = await service.GetAllAsync(usuarioId);
            return Results.Ok(orders);
        })
        .WithName("GetAllOrders")
        .WithSummary("Listar órdenes")
        .WithDescription("Devuelve el listado de órdenes. Permite filtrar opcionalmente por usuarioId.")
        .Produces<IEnumerable<OrderResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET: Obtener detalle de una orden por ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            OrderService service) =>
        {
            var order = await service.GetByIdAsync(id);
            return Results.Ok(order);
        })
        .WithName("GetOrderById")
        .WithSummary("Obtener detalle de una orden")
        .WithDescription("Devuelve los datos de la orden identificada por el GUID indicado (ORD-001).")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // POST: Crear nueva orden
        group.MapPost("/", async (
            CreateOrderRequest request,
            OrderService service) =>
        {
            var order = await service.CreateAsync(request);
            return Results.Created($"/api/orders/{order.Id}", order);
        })
        .WithName("CreateOrder")
        .WithSummary("Crear nueva orden")
        .WithDescription("Valida datos (ORD-002), usuario (ORD-003), productos (ORD-004) y stock (ORD-005).")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT: Actualizar estado de la orden
        group.MapPut("/{id:guid}/status", async (
            Guid id,
            UpdateOrderStatusRequest request,
            OrderService service) =>
        {
            var order = await service.UpdateStatusAsync(id, request);
            return Results.Ok(order);
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Actualizar estado de la orden")
        .WithDescription("Modifica el estado de la orden validando existencia (ORD-001) y transiciones válidas (ORD-006).")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}