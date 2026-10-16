using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;

namespace MiniApi.Data
{
    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // Fila de cart_items tal como sale de la base
        private class CartItemRow
        {
            public string ProductoId { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        // ── GET BY USER ───────────────────────────────────────────────────────
        // Devuelve null si el usuario no tiene carrito.
        public async Task<Cart?> GetByUserIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var id = usuarioId.ToString();

            var fecha = await conn.QuerySingleOrDefaultAsync<string>(
                "SELECT fecha_actualizacion FROM cart WHERE usuario_id = @id", new { id });
            if (fecha is null) return null;

            var filas = await conn.QueryAsync<CartItemRow>("""
                SELECT producto_id AS ProductoId, cantidad AS Cantidad
                FROM cart_items
                WHERE usuario_id = @id
            """, new { id });

            var items = new List<CartItem>();
            foreach (var fila in filas)
                items.Add(new CartItem(Guid.Parse(fila.ProductoId), fila.Cantidad));

            return new Cart(usuarioId, items.ToArray(),
                DateTime.Parse(fecha, null, DateTimeStyles.RoundtripKind));
        }

        // ── SET ITEM ──────────────────────────────────────────────────────────
        // Crea el carrito si no existe y deja el item con la cantidad indicada.
        public async Task SetItemAsync(Guid usuarioId, Guid productoId, int cantidad)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            var parametros = new
            {
                usuario = usuarioId.ToString(),
                producto = productoId.ToString(),
                cantidad,
                fecha = DateTime.UtcNow.ToString("o")
            };

            await conn.ExecuteAsync("""
                INSERT INTO cart (usuario_id, fecha_actualizacion) VALUES (@usuario, @fecha)
                ON CONFLICT (usuario_id) DO UPDATE SET fecha_actualizacion = @fecha
            """, parametros, tx);

            await conn.ExecuteAsync("""
                INSERT INTO cart_items (usuario_id, producto_id, cantidad) VALUES (@usuario, @producto, @cantidad)
                ON CONFLICT (usuario_id, producto_id) DO UPDATE SET cantidad = @cantidad
            """, parametros, tx);

            tx.Commit();
        }

        // ── REMOVE ITEM ───────────────────────────────────────────────────────
        // Devuelve false si el item no estaba en el carrito.
        public async Task<bool> RemoveItemAsync(Guid usuarioId, Guid productoId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            var rows = await conn.ExecuteAsync(
                "DELETE FROM cart_items WHERE usuario_id = @usuario AND producto_id = @producto",
                new { usuario = usuarioId.ToString(), producto = productoId.ToString() }, tx);

            if (rows > 0)
                await conn.ExecuteAsync(
                    "UPDATE cart SET fecha_actualizacion = @fecha WHERE usuario_id = @usuario",
                    new { usuario = usuarioId.ToString(), fecha = DateTime.UtcNow.ToString("o") }, tx);

            tx.Commit();
            return rows > 0;
        }

        // ── CLEAR ─────────────────────────────────────────────────────────────
        // Borra el carrito completo. Devuelve false si el usuario no tenía carrito.
        public async Task<bool> ClearAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var id = usuarioId.ToString();

            await conn.ExecuteAsync("DELETE FROM cart_items WHERE usuario_id = @id", new { id }, tx);
            var rows = await conn.ExecuteAsync("DELETE FROM cart WHERE usuario_id = @id", new { id }, tx);

            tx.Commit();
            return rows > 0;
        }
    }
}
