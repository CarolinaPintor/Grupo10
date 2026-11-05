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

        // Estados permitidos para una orden
        private static readonly string[] EstadosValidos = { "Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada" };

        public OrderService(
            OrderRepository orderRepository,
            ProductRepository productRepository,
            UserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId)
        {
            var orders = await _orderRepository.GetAllAsync(usuarioId);

            var respuestas = new List<OrderResponse>();
            foreach (var o in orders)
            {
                respuestas.Add(MapToResponse(o));
            }

            return respuestas;
        }

        public async Task<OrderResponse> GetByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada."); // ORD-001[cite: 12]
            }

            return MapToResponse(order);
        }

        // Convierte una orden del modelo en el DTO de respuesta (con foreach en vez de LINQ)
        private static OrderResponse MapToResponse(Order order)
        {
            var items = new List<OrderItemResponse>();
            foreach (var i in order.Items)
            {
                items.Add(new OrderItemResponse(i.ProductoId, i.Cantidad, i.PrecioUnitario));
            }

            return new OrderResponse(
                order.Id,
                order.UsuarioId,
                items,
                order.Total,
                order.Estado,
                order.FechaCreacion
            );
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
        {
            // ORD-002: Validar formato de datos o lista de ítems vacía con Data Annotations / Reglas
            List<ValidationResult> resultadosValidacion = new List<ValidationResult>();
            ValidationContext contextoValidacion = new ValidationContext(request);
            bool esValido = Validator.TryValidateObject(request, contextoValidacion, resultadosValidacion, true);

            bool sinItems = request.Items == null || request.Items.Count == 0;

            if (!esValido || request.UsuarioId == Guid.Empty || sinItems)
            {
                var errores = new List<string>();

                if (sinItems)
                {
                    errores.Add("Los datos de la orden son inválidos.");
                }
                else if (request.UsuarioId == Guid.Empty)
                {
                    errores.Add("El ID del usuario es obligatorio.");
                }
                else
                {
                    foreach (var r in resultadosValidacion)
                    {
                        errores.Add(r.ErrorMessage ?? "Dato inválido");
                    }
                }

                throw new ValidationException(ErrorCodes.ORD_002, errores);
            }

            // ORD-002: Validator no revisa los ítems anidados, así que valido cada cantidad acá
            foreach (CreateOrderItemRequest item in request.Items)
            {
                if (item.Cantidad < 1)
                {
                    throw new ValidationException(ErrorCodes.ORD_002, new[] { "La cantidad de cada ítem debe ser al menos 1." });
                }
            }

            // ORD-003: Validar que el usuario exista y esté activo (Catálogo: "Usuario no encontrado al crear la orden.")
            User? user = await _userRepository.GetByIdAsync(request.UsuarioId);
            if (user == null || !user.Activo)
            {
                throw new NotFoundException(ErrorCodes.ORD_003, $"El usuario con ID '{request.UsuarioId}' no existe o no está activo."); // ORD-003[cite: 12]
            }

            // ORD-002 / ORD-004: Validar que la orden contenga ítems
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new BusinessRuleException(ErrorCodes.ORD_002, "La orden debe contener al menos un ítem."); // ORD-002[cite: 12]
            }

            var orderItems = new List<OrderItem>();

            // Recorrer con foreach y acumulador, validando productos y stock
            foreach (var itemDto in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductoId);

                // ORD-004: Producto no encontrado al crear la orden[cite: 12]
                if (product == null)
                {
                    throw new NotFoundException(ErrorCodes.ORD_004, $"No se encontró el producto con ID '{itemDto.ProductoId}'."); // ORD-004[cite: 12]
                }

                // ORD-005: Stock insuficiente con Status422UnprocessableEntity[cite: 13]
                if (product.Stock < itemDto.Cantidad)
                {
                    throw new BusinessRuleException(
                        ErrorCodes.ORD_005,
                        $"Stock insuficiente para el producto '{product.Nombre}'. Stock disponible: {product.Stock}, solicitado: {itemDto.Cantidad}.", // ORD-005[cite: 13]
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
            return MapToResponse(nuevaOrden);
        }

        public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            // ORD-002: el estado debe ser uno de los permitidos
            bool estadoValido = false;
            foreach (string estado in EstadosValidos)
            {
                if (estado == request.Estado)
                {
                    estadoValido = true;
                }
            }

            if (!estadoValido)
            {
                throw new ValidationException(ErrorCodes.ORD_002, new[]
                {
                    $"El estado '{request.Estado}' no es válido. Valores permitidos: {string.Join(", ", EstadosValidos)}."
                });
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada."); // ORD-001[cite: 12]
            }

            // ORD-006: Validar transición de estados (Ej: Entregada no puede volver a Pendiente)[cite: 14]
            if (order.Estado == "Entregada" && request.Estado == "Pendiente")
            {
                throw new BusinessRuleException(
                    ErrorCodes.ORD_006,
                    "Una orden en estado 'Entregada' no puede volver a 'Pendiente'.", // ORD-006[cite: 14]
                    StatusCodes.Status409Conflict
                );
            }

            order.Estado = request.Estado;
            await _orderRepository.UpdateAsync(order);

            return new UpdateOrderStatusResponse(order.Id, order.Estado, DateTime.UtcNow);
        }
    }
}