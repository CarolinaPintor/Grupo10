using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class ProductEndpointsExtension
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
                       .WithTags("Products");

        // GET all (con soporte para filtros opcionales de categoría y nombre)
        group.MapGet("/", async (ProductService service, string? categoria, string? nombre) =>
        {
            var products = await service.GetAllAsync(categoria, nombre);
            return Results.Ok(products);
        });

        // GET by id
        group.MapGet("/{id:guid}", async (Guid id, ProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return Results.Ok(product);
        });

        // POST
        group.MapPost("/", async (CreateProductRequest request, ProductService service) =>
        {
            var product = await service.CreateAsync(request);
            return Results.Created($"/api/products/{product.Id}", product);
        });

        // PUT
        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, ProductService service) =>
        {
            var product = await service.UpdateAsync(id, request);
            return Results.Ok(product);
        });

        // DELETE
        group.MapDelete("/{id:guid}", async (Guid id, ProductService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}