using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;
using MiniApi.Models;
using ValidationException = MiniApi.Exceptions.ValidationException;

namespace MiniApi.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;

        public OrderService(
            OrderRepository orderRepository,
            ProductRepository productRepository,
            UserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
        {
            // ORD-002: Validar formato de datos con Data Annotations
            List<ValidationResult> resultadosValidacion = new List<ValidationResult>();
            ValidationContext contextoValidacion = new ValidationContext(request);
            bool esValido = Validator.TryValidateObject(request, contextoValidacion, resultadosValidacion, true);

            if (!esValido)
            {
                IEnumerable<string> errores = resultadosValidacion.Select(r => r.ErrorMessage ?? "Dato inválido");
                throw new ValidationException(ErrorCodes.ORD_002, errores);
            }

            // ORD-003: Validar que el usuario exista y esté activo
            User? user = await _userRepository.GetByIdAsync(request.UsuarioId);
            if (user == null || !user.Activo)
            {
                throw new NotFoundException(ErrorCodes.ORD_003, $"El usuario con ID '{request.UsuarioId}' no existe o no está activo.");
            }

            // ORD-004: Validar que la orden contenga ítems
            if (request.Items == null || !request.Items.Any())
            {
                throw new BusinessRuleException(ErrorCodes.ORD_004, "La orden debe contener al menos un ítem.");
            }

            var orderItems = new List<OrderItem>();

            // Recorrer con foreach y acumulador, validando productos y stock
            foreach (var itemDto in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductoId);

                // Producto inexistente corregido a ORD-004 (404)
                if (product == null)
                {
                    throw new NotFoundException(ErrorCodes.ORD_004, $"No se encontró el producto con ID '{itemDto.ProductoId}'.");
                }

                // Stock insuficiente con Status422UnprocessableEntity como tercer parámetro
                if (product.Stock < itemDto.Cantidad)
                {
                    throw new BusinessRuleException(
                        ErrorCodes.ORD_005,
                        $"Stock insuficiente para el producto '{product.Nombre}'. Stock disponible: {product.Stock}, solicitado: {itemDto.Cantidad}.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }

                orderItems.Add(new OrderItem(product.Id, itemDto.Cantidad, product.Precio));
            }

            // Utilizar el constructor de Order para inicializar la orden, convertir los ítems y calcular el total automáticamente
            Order nuevaOrden = new Order(request.UsuarioId, orderItems);

            // Guardar la orden en la base de datos
            await _orderRepository.AddAsync(nuevaOrden);

            // Retornar el DTO de respuesta seguro
            return new OrderResponse(
                nuevaOrden.Id,
                nuevaOrden.UsuarioId,
                nuevaOrden.Items.Select(i => new OrderItemResponse(i.ProductoId, i.Cantidad, i.PrecioUnitario)).ToList(),
                nuevaOrden.Total,
                nuevaOrden.Estado,
                nuevaOrden.FechaCreacion
            );
        }
    }
}