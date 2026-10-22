using System.ComponentModel.DataAnnotations;

namespace MiniApi.DTOs;

// Request para crear un producto (POST /api/products)
public record CreateProductRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    string Nombre,

    string? Descripcion,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    decimal Precio,

    [property: Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int Stock,

    [property: Required(ErrorMessage = "La categoría es obligatoria.")]
    string Categoria
);

// Request para actualizar un producto (PUT /api/products/{id})
public record UpdateProductRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    string Nombre,

    string? Descripcion,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    decimal Precio,

    [property: Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int Stock,

    [property: Required(ErrorMessage = "La categoría es obligatoria.")]
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