
using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class ProductEndpointsExtension
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
                       .WithTags("Products");

        // GET: obtener todos los productos con filtros opcionales
        group.MapGet("/", async (
            ProductService service,
            string? categoria,
            string? nombre) =>
        {
            var products = await service.GetAllAsync(categoria, nombre);
            return Results.Ok(products);
        })
        .WithName("GetAllProducts")
        .WithSummary("Obtener todos los productos")
        .WithDescription(
            "Devuelve los productos registrados. Permite filtrar " +
            "opcionalmente por categoría y nombre.")
        .Produces<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET: obtener un producto por ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return Results.Ok(product);
        })
        .WithName("GetProductById")
        .WithSummary("Obtener un producto por ID")
        .WithDescription(
            "Devuelve los datos del producto identificado por el GUID indicado.")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // POST: crear un producto
        group.MapPost("/", async (
            CreateProductRequest request,
            ProductService service) =>
        {
            var product = await service.CreateAsync(request);

            return Results.Created(
                $"/api/products/{product.Id}",
                product);
        })
        .WithName("CreateProduct")
        .WithSummary("Crear un nuevo producto")
        .WithDescription(
            "Valida los datos, comprueba que no exista otro producto " +
            "con el mismo nombre en la categoría y registra el producto.")
        .Produces<ProductResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT: actualizar un producto
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            ProductService service) =>
        {
            var product = await service.UpdateAsync(id, request);
            return Results.Ok(product);
        })
        .WithName("UpdateProduct")
        .WithSummary("Actualizar un producto existente")
        .WithDescription(
            "Actualiza los datos del producto indicado por su GUID.")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE: eliminar un producto
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ProductService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteProduct")
        .WithSummary("Eliminar un producto existente")
        .WithDescription(
            "Elimina el producto indicado por su GUID si existe.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
//Descubrí como agregar los ejemplos del swagger, ya fueron agregados y he de testearlos.
