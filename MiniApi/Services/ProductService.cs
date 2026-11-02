using System.ComponentModel.DataAnnotations;
using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;
using MiniApi.Models;

// Hay otra ValidationException en DataAnnotations: se indica cuál usar
using ValidationException = MiniApi.Exceptions.ValidationException;

namespace MiniApi.Services;

public class ProductService
{
    private readonly ProductRepository _repository;
    private readonly OrderRepository _orderRepository;

    public ProductService(ProductRepository repository, OrderRepository orderRepository)
    {
        _repository = repository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
    {
        var products = await _repository.GetAllAsync(categoria, nombre);
        return products.Select(MapToResponse);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            throw new NotFoundException(ErrorCodes.PRD_001, $"No se encontró el producto con ID '{id}'.");
        }

        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        // Validar formato de datos con Data Annotations (PRD-002)
        ValidateRequest(request);

        // Validar regla de nombre duplicado en misma categoría (PRD-003)
        var exists = await _repository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria);
        if (exists)
        {
            throw new BusinessRuleException(ErrorCodes.PRD_003, $"Ya existe un producto con el nombre '{request.Nombre}' en la categoría '{request.Categoria}'.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.AddAsync(product);
        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        // Verificar existencia (PRD-001)
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            throw new NotFoundException(ErrorCodes.PRD_001, $"No se encontró el producto con ID '{id}'.");
        }

        // Validar formato de datos (PRD-002)
        ValidateRequest(request);

        // Validar duplicidad en cambio de nombre/categoría (PRD-003)
        var nombreCambio = !product.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase);
        var categoriaCambio = !product.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase);

        if ((nombreCambio || categoriaCambio) && await _repository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria))
        {
            throw new BusinessRuleException(ErrorCodes.PRD_003, $"Ya existe un producto con el nombre '{request.Nombre}' en la categoría '{request.Categoria}'.");
        }

        product.Nombre = request.Nombre.Trim();
        product.Descripcion = request.Descripcion?.Trim();
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria.Trim();

        await _repository.UpdateAsync(product);
        return MapToResponse(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            throw new NotFoundException(ErrorCodes.PRD_001, $"No se encontró el producto con ID '{id}'.");
        }

        // PRD-004: no se puede eliminar un producto con órdenes Pendiente o Confirmada
        if (await _orderRepository.HasActiveOrdersByProductAsync(id))
        {
            throw new BusinessRuleException(ErrorCodes.PRD_004, "El producto tiene órdenes activas y no puede eliminarse.");
        }

        await _repository.DeleteAsync(id);
    }

    private static void ValidateRequest<T>(T request) where T : class
    {
        var resultadosValidacion = new List<ValidationResult>();
        var contextoValidacion = new ValidationContext(request);
        bool esValido = Validator.TryValidateObject(request, contextoValidacion, resultadosValidacion, true);

        if (!esValido)
        {
            var errores = resultadosValidacion.Select(r => r.ErrorMessage ?? "Dato inválido");
            throw new ValidationException(ErrorCodes.PRD_002, errores);
        }
    }

    private static ProductResponse MapToResponse(Product product) => new(
        product.Id,
        product.Nombre,
        product.Descripcion,
        product.Precio,
        product.Stock,
        product.Categoria,
        product.FechaCreacion
    );
}