using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");
        }

        // Fila intermedia para mapear la orden correctamente
        private class OrderRow
        {
            public string Id { get; set; } = string.Empty;
            public string UsuarioId { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;

            public Order ToModel(IEnumerable<OrderItem> items)
            {
                var order = new Order
                {
                    Id = Guid.Parse(Id),
                    UsuarioId = Guid.Parse(UsuarioId),
                    Total = Total,
                    Estado = Estado,
                    FechaCreacion = DateTime.Parse(FechaCreacion, null, DateTimeStyles.RoundtripKind)
                };

                var listaItems = new List<OrderItem>();
                foreach (OrderItem item in items)
                {
                    listaItems.Add(item);
                }

                order.Items = listaItems.ToArray();
                return order;
            }
        }

        // Fila intermedia para los ítems de la orden
        private class OrderItemRow
        {
            public string ProductoId { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }

            public OrderItem ToModel()
            {
                return new OrderItem(Guid.Parse(ProductoId), Cantidad, PrecioUnitario);
            }
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var orderRow = await conn.QuerySingleOrDefaultAsync<OrderRow>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, 
                       estado AS Estado, fecha_creacion AS FechaCreacion 
                FROM orders 
                WHERE id = @id
            """, new { id = id.ToString() });

            if (orderRow == null) return null;

            var itemRows = await conn.QueryAsync<OrderItemRow>("""
                SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario 
                FROM order_items 
                WHERE order_id = @orderId
            """, new { orderId = id.ToString() });

            var items = new List<OrderItem>();
            foreach (var fila in itemRows)
            {
                items.Add(fila.ToModel());
            }

            return orderRow.ToModel(items);
        }

        // ── GET BY USER ID ────────────────────────────────────────────────────
        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();

            var orderRows = await conn.QueryAsync<OrderRow>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, 
                       estado AS Estado, fecha_creacion AS FechaCreacion 
                FROM orders 
                WHERE usuario_id = @usuarioId
            """, new { usuarioId = usuarioId.ToString() });

            var orders = new List<Order>();

            foreach (var orderRow in orderRows)
            {
                var itemRows = await conn.QueryAsync<OrderItemRow>("""
                    SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario 
                    FROM order_items 
                    WHERE order_id = @orderId
                """, new { orderId = orderRow.Id });

                var items = new List<OrderItem>();
                foreach (var fila in itemRows)
                {
                    items.Add(fila.ToModel());
                }

                orders.Add(orderRow.ToModel(items));
            }

            return orders;
        }

        // ── GET ALL (Con filtro opcional por usuarioId) ───────────────────────
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {
            if (usuarioId.HasValue)
            {
                return await GetByUserIdAsync(usuarioId.Value);
            }

            using var conn = CreateConnection();

            var orderRows = await conn.QueryAsync<OrderRow>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, 
                       estado AS Estado, fecha_creacion AS FechaCreacion 
                FROM orders
            """);

            var orders = new List<Order>();

            foreach (var orderRow in orderRows)
            {
                var itemRows = await conn.QueryAsync<OrderItemRow>("""
                    SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario 
                    FROM order_items 
                    WHERE order_id = @orderId
                """, new { orderId = orderRow.Id });

                var items = new List<OrderItem>();
                foreach (var fila in itemRows)
                {
                    items.Add(fila.ToModel());
                }

                orders.Add(orderRow.ToModel(items));
            }

            return orders;
        }

        // ── ADD (Inserta orden, ítems y descuenta stock en transacción) ───────
        public async Task AddAsync(Order order)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync("""
                    INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
                    VALUES (@id, @usuarioId, @total, @estado, @fecha)
                """, new
                {
                    id = order.Id.ToString(),
                    usuarioId = order.UsuarioId.ToString(),
                    total = order.Total,
                    estado = order.Estado,
                    fecha = order.FechaCreacion.ToString("o")
                }, transaction);

                foreach (var item in order.Items)
                {
                    await conn.ExecuteAsync("""
                        INSERT INTO order_items (id, order_id, producto_id, cantidad, precio_unitario)
                        VALUES (@itemId, @orderId, @productoId, @cantidad, @precioUnitario)
                    """, new
                    {
                        itemId = Guid.NewGuid().ToString(),
                        orderId = order.Id.ToString(),
                        productoId = item.ProductoId.ToString(),
                        cantidad = item.Cantidad,
                        precioUnitario = item.PrecioUnitario
                    }, transaction);

                    // Descontar el stock del producto comprado
                    await conn.ExecuteAsync("""
                        UPDATE products 
                        SET stock = stock - @cantidad 
                        WHERE id = @productoId
                    """, new
                    {
                        cantidad = item.Cantidad,
                        productoId = item.ProductoId.ToString()
                    }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ── UPDATE STATUS ─────────────────────────────────────────────────────
        public async Task UpdateStatusAsync(Guid id, string nuevoEstado)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                UPDATE orders 
                SET estado = @estado 
                WHERE id = @id
            """, new { id = id.ToString(), estado = nuevoEstado });
        }

        // ── UPDATE (Compatible con OrderService) ──────────────────────────────
        public async Task UpdateAsync(Order order)
        {
            await UpdateStatusAsync(order.Id, order.Estado);
        }

        // ── HAS ACTIVE ORDERS BY PRODUCT (PRD-004) ────────────────────────────
        public async Task<bool> HasActiveOrdersByProductAsync(Guid productId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>("""
                SELECT COUNT(1) 
                FROM order_items oi
                JOIN orders o ON oi.order_id = o.id
                WHERE oi.producto_id = @productId AND o.estado IN ('Pendiente', 'Confirmada')
            """, new { productId = productId.ToString() });

            return count > 0;
        }

        // ── HAS ORDERS BY PRODUCT (cualquier estado) ──────────────────────────
        public async Task<bool> HasOrdersByProductAsync(Guid productId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM order_items WHERE producto_id = @productId",
                new { productId = productId.ToString() });

            return count > 0;
        }
    }
}