using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data;

public class NotificationRepository
{
    private readonly IConfiguration _config;

    public NotificationRepository(IConfiguration config)
    {
        _config = config;
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");
    }

    // 1. Obtener todas las notificaciones (ordenadas de la más reciente a la más antigua)
    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = """
            SELECT 
                id AS Id,
                usuario_id AS UsuarioId,
                mensaje AS Mensaje,
                tipo AS Tipo,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            ORDER BY fecha_envio DESC;
            """;

        return await connection.QueryAsync<Notification>(sql);
    }

    // 2. Obtener todas las notificaciones de un usuario específico
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();
        const string sql = """
            SELECT 
                id AS Id,
                usuario_id AS UsuarioId,
                mensaje AS Mensaje,
                tipo AS Tipo,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC;
            """;

        //Se pasa usuarioId.ToString() para que coincida con el TEXT de SQLite
        return await connection.QueryAsync<Notification>(sql, new { UsuarioId = usuarioId.ToString() });
    }

    // 3. Obtener una notificación puntual por su Id
    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        const string sql = """
            SELECT 
                id AS Id,
                usuario_id AS UsuarioId,
                mensaje AS Mensaje,
                tipo AS Tipo,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            WHERE id = @Id;
            """;

        //Se pasa id.ToString() para que coincida con el TEXT de SQLite
        return await connection.QueryFirstOrDefaultAsync<Notification>(sql, new { Id = id.ToString() });
    }

    // 4. Guardar una nueva notificación en la base de datos
    public async Task CreateAsync(Notification notification)
    {
        using var connection = CreateConnection();
        const string sql = """
            INSERT INTO notifications (
                id, 
                usuario_id, 
                mensaje, 
                tipo, 
                estado, 
                fecha_envio
            )
            VALUES (
                @Id, 
                @UsuarioId, 
                @Mensaje, 
                @Tipo, 
                @Estado, 
                @FechaEnvio
            );
            """;

        // Modifico algo: Guardo Id y UsuarioId como string (.ToString()) en SQLite
        await connection.ExecuteAsync(sql, new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,
            FechaEnvio = notification.FechaEnvio.ToString("o")
        });
    }
}