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
            //tabla de usuarios
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

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}