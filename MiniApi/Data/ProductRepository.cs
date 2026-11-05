using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
{
    public class ProductRepository
    {
        private readonly IConfiguration _config;

        public ProductRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // Fila tal como sale de la base para mapear tipos correctamente
        private class ProductRow
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public int Stock { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;

            public Product ToModel() => new()
            {
                Id = Guid.Parse(Id),
                Nombre = Nombre,
                Descripcion = Descripcion,
                Precio = Precio,
                Stock = Stock,
                Categoria = Categoria,
                FechaCreacion = DateTime.Parse(FechaCreacion, null, DateTimeStyles.RoundtripKind)
            };
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
        {
            using var conn = CreateConnection();
            var sql = """
                SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, 
                       precio AS Precio, stock AS Stock, categoria AS Categoria, 
                       fecha_creacion AS FechaCreacion 
                FROM products 
                WHERE 1=1
            """;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                sql += " AND LOWER(categoria) = LOWER(@categoria)";
                parameters.Add("categoria", categoria);
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                sql += " AND LOWER(nombre) LIKE LOWER(@nombre)";
                parameters.Add("nombre", $"%{nombre}%");
            }

            var filas = await conn.QueryAsync<ProductRow>(sql, parameters);

            var productos = new List<Product>();
            foreach (var fila in filas)
            {
                productos.Add(fila.ToModel());
            }

            return productos;
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var fila = await conn.QuerySingleOrDefaultAsync<ProductRow>("""
                SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, 
                       precio AS Precio, stock AS Stock, categoria AS Categoria, 
                       fecha_creacion AS FechaCreacion 
                FROM products 
                WHERE id = @id
            """, new { id = id.ToString() });

            return fila?.ToModel();
        }

        // ── EXISTS BY NAME AND CATEGORY ───────────────────────────────────────
        public async Task<bool> ExistsByNameAndCategoryAsync(string nombre, string categoria)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>("""
                SELECT COUNT(1) 
                FROM products 
                WHERE LOWER(nombre) = LOWER(@nombre) AND LOWER(categoria) = LOWER(@categoria)
            """, new { nombre, categoria });

            return count > 0;
        }

        // ── ADD ───────────────────────────────────────────────────────────────
        public async Task AddAsync(Product product)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
                VALUES (@id, @nombre, @descripcion, @precio, @stock, @categoria, @fecha)
            """, new
            {
                id = product.Id.ToString(),
                nombre = product.Nombre,
                descripcion = product.Descripcion,
                precio = product.Precio,
                stock = product.Stock,
                categoria = product.Categoria,
                fecha = product.FechaCreacion.ToString("o")
            });
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Product product)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                UPDATE products 
                SET nombre = @nombre, 
                    descripcion = @descripcion, 
                    precio = @precio, 
                    stock = @stock, 
                    categoria = @categoria
                WHERE id = @id
            """, new
            {
                id = product.Id.ToString(),
                nombre = product.Nombre,
                descripcion = product.Descripcion,
                precio = product.Precio,
                stock = product.Stock,
                categoria = product.Categoria
            });
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("DELETE FROM products WHERE id = @id", new { id = id.ToString() });
        }
    }
}