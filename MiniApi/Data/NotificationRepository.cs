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

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

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

        return await connection.QueryAsync<Notification>(sql, new { UsuarioId = usuarioId });
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

        return await connection.QueryFirstOrDefaultAsync<Notification>(sql, new { Id = id });
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

        await connection.ExecuteAsync(sql, new
        {
            notification.Id,
            notification.UsuarioId,
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,
            FechaEnvio = notification.FechaEnvio.ToString("o")
        });
    }
}