using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;

namespace MiniApi.Services;

public class CartService
{
    private readonly CartRepository _repository;
    private readonly ProductService _productService;

    public CartService(CartRepository repository, ProductService productService)
    {
        _repository = repository;
        _productService = productService;
    }

    public async Task<CartResponse> GetAsync(Guid usuarioId)
    {
        var cart = await ObtenerCarritoAsync(usuarioId);
        return MapToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(Guid usuarioId, AddCartItemRequest request)
    {
        // Cantidad inválida (CRT-004)
        ValidarCantidad(request.Cantidad);

        // Producto inexistente (CRT-002)
        var producto = await ObtenerProductoAsync(request.ProductoId);

        // Si el producto ya está en el carrito, la cantidad se suma
        var cantidadTotal = request.Cantidad;
        var cart = await _repository.GetByUserIdAsync(usuarioId);
        if (cart is not null)
        {
            foreach (var item in cart.Items)
            {
                if (item.ProductoId == request.ProductoId)
                {
                    cantidadTotal += item.Cantidad;
                }
            }
        }

        // Stock insuficiente (CRT-003)
        ValidarStock(producto.Stock, cantidadTotal);

        await _repository.SetItemAsync(usuarioId, request.ProductoId, cantidadTotal);
        return await GetAsync(usuarioId);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid usuarioId, Guid productoId, UpdateCartItemRequest request)
    {
        ValidarCantidad(request.Cantidad);

        var cart = await ObtenerCarritoAsync(usuarioId);
        var producto = await ObtenerProductoAsync(productoId);

        var estaEnElCarrito = false;
        foreach (var item in cart.Items)
        {
            if (item.ProductoId == productoId)
            {
                estaEnElCarrito = true;
            }
        }
        if (!estaEnElCarrito)
        {
            throw ProductoFueraDelCarrito(productoId);
        }

        ValidarStock(producto.Stock, request.Cantidad);

        await _repository.SetItemAsync(usuarioId, productoId, request.Cantidad);
        return await GetAsync(usuarioId);
    }

    public async Task RemoveItemAsync(Guid usuarioId, Guid productoId)
    {
        await ObtenerCarritoAsync(usuarioId);

        var eliminado = await _repository.RemoveItemAsync(usuarioId, productoId);
        if (!eliminado)
        {
            throw ProductoFueraDelCarrito(productoId);
        }
    }

    public async Task ClearAsync(Guid usuarioId)
    {
        var eliminado = await _repository.ClearAsync(usuarioId);
        if (!eliminado)
        {
            throw CarritoNoEncontrado(usuarioId);
        }
    }

    private async Task<Cart> ObtenerCarritoAsync(Guid usuarioId)
    {
        var cart = await _repository.GetByUserIdAsync(usuarioId);
        if (cart is null)
        {
            throw CarritoNoEncontrado(usuarioId);
        }

        return cart;
    }

    // ProductService informa PRD-001; para el carrito el código que corresponde es CRT-002
    private async Task<ProductResponse> ObtenerProductoAsync(Guid productoId)
    {
        try
        {
            return await _productService.GetByIdAsync(productoId);
        }
        catch (NotFoundException)
        {
            throw new NotFoundException(ErrorCodes.CRT_002, $"No se encontró el producto con ID '{productoId}'.");
        }
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ValidationException(ErrorCodes.CRT_004, new[] { "La cantidad debe ser mayor a cero." });
        }
    }

    private static void ValidarStock(int stockDisponible, int cantidadSolicitada)
    {
        if (cantidadSolicitada > stockDisponible)
        {
            throw new BusinessRuleException(
                ErrorCodes.CRT_003,
                $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {cantidadSolicitada}.",
                StatusCodes.Status422UnprocessableEntity);
        }
    }

    private static NotFoundException CarritoNoEncontrado(Guid usuarioId) =>
        new(ErrorCodes.CRT_001, $"El usuario '{usuarioId}' no tiene un carrito activo.");

    private static NotFoundException ProductoFueraDelCarrito(Guid productoId) =>
        new(ErrorCodes.CRT_002, $"El producto '{productoId}' no está en el carrito.");

    private static CartResponse MapToResponse(Cart cart)
    {
        var items = new List<CartItemResponse>();
        foreach (var item in cart.Items)
        {
            items.Add(new CartItemResponse(item.ProductoId, item.Cantidad));
        }

        return new CartResponse(cart.UsuarioId, items, cart.FechaActualizacion);
    }
}
