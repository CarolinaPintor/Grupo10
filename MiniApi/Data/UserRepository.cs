using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GET BY EMAIL ───────────────────────────────────────────────────────
        // Necesario para el registro y para la autenticación en el login.
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT id                 AS Id,
                       nombre             AS Nombre,
                       apellido           AS Apellido,
                       email              AS Email,
                       password_hash      AS PasswordHash,
                       fecha_registro     AS FechaRegistro,
                       activo             AS Activo,
                       intentos_fallidos  AS IntentosFallidos
                FROM users
                WHERE email = @email
            """, new { email });
        }

        // ── GET BY ID ──────────────────────────────────────────────────────────
        // Necesario para obtener información del usuario autenticado a partir del token JWT.
        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT id                 AS Id,
                       nombre             AS Nombre,
                       apellido           AS Apellido,
                       email              AS Email,
                       password_hash      AS PasswordHash,
                       fecha_registro     AS FechaRegistro,
                       activo             AS Activo,
                       intentos_fallidos  AS IntentosFallidos
                FROM users
                WHERE id = @id
            """, new { id = id.ToString() });
        }

        // ── CREATE ─────────────────────────────────────────────────────────────
        public async Task CreateAsync(User user)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
            """, new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("o"),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });
        }

        // ── UPDATE ─────────────────────────────────────────────────────────────
        // Necesario para actualizar IntentosFallidos y Activo tras logins exitosos o fallidos.
        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
                UPDATE users
                SET nombre            = @Nombre,
                    apellido          = @Apellido,
                    email             = @Email,
                    password_hash     = @PasswordHash,
                    activo            = @Activo,
                    intentos_fallidos = @IntentosFallidos
                WHERE id = @Id
            """, new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });

            return rows > 0;
        }
    }
}