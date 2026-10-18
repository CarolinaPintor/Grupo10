namespace MiniApi.DTOs;

// Request para crear un producto (POST /api/products)
public record CreateProductRequest(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    string Categoria
);

// Request para actualizar un producto (PUT /api/products/{id})
public record UpdateProductRequest(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    string Categoria
);

// Respuesta con los datos completos del producto
public record ProductResponse(
    Guid Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    string Categoria,
    DateTime FechaCreacion
);