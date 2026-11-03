using System.ComponentModel.DataAnnotations;

namespace MiniApi.DTOs;

// Request para crear un producto (POST /api/products)
public record CreateProductRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    [property: StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    decimal Precio,

    [property: Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int Stock,

    [property: Required(ErrorMessage = "La categoría es obligatoria.")]
    [property: StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
    string Categoria
);

// Request para actualizar un producto (PUT /api/products/{id})
public record UpdateProductRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    [property: StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    decimal Precio,

    [property: Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int Stock,

    [property: Required(ErrorMessage = "La categoría es obligatoria.")]
    [property: StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
    string Categoria
);

// Respuesta con los datos completos del producto
public record ProductResponse(
    Guid Id,
    string Nombre,
    [property: StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,
    decimal Precio,
    int Stock,
    string Categoria,
    DateTime FechaCreacion
);