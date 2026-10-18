using Dapper;
using Microsoft.Data.Sqlite;

namespace MiniApi.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // ALUMNO: en caso de tener que agregar otra tabla es duplicar este comando
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS items (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                name        TEXT    NOT NULL,
                description TEXT,
                price       REAL    NOT NULL DEFAULT 0,
                stock       INTEGER NOT NULL DEFAULT 0,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at  TEXT
            );
        """);

            // Tabla de productos
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id             TEXT PRIMARY KEY,
                nombre         TEXT NOT NULL,
                descripcion    TEXT,
                precio         REAL NOT NULL,
                stock          INTEGER NOT NULL,
                categoria      TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
            """);

            // Tabla de usuarios
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id                 TEXT    PRIMARY KEY,
                nombre             TEXT    NOT NULL,
                apellido           TEXT    NOT NULL,
                email              TEXT    NOT NULL UNIQUE,
                password_hash      TEXT    NOT NULL,
                fecha_registro     TEXT    NOT NULL,
                activo             INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos  INTEGER NOT NULL DEFAULT 0
            );
            """);

            // Tabla de carritos (un carrito por usuario)
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS cart (
                usuario_id           TEXT PRIMARY KEY,
                fecha_actualizacion  TEXT NOT NULL
            );
            """);

            // Tabla de items del carrito
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS cart_items (
                usuario_id   TEXT    NOT NULL,
                producto_id  TEXT    NOT NULL,
                cantidad     INTEGER NOT NULL CHECK (cantidad > 0),
                PRIMARY KEY (usuario_id, producto_id),
                FOREIGN KEY (usuario_id) REFERENCES cart (usuario_id)
            );
            """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}