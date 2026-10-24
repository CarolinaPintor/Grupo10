using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.Services;

public class NotificationService
{
    private static readonly string[] TiposPermitidos = ["Email", "Push", "SMS"];

    private readonly NotificationRepository _notificationRepository;
    private readonly UserRepository _userRepository;

    public NotificationService(
        NotificationRepository notificationRepository,
        UserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }

    // Lógica de negocio para enviar/registrar una notificación (NTF-001 y NTF-002)
    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
    {
        // 1. Validación de datos de entrada (NTF-002 -> HTTP 400 Bad Request)
        ValidarRequest(request);

        // 2. Verificar que el usuario destinatario exista (NTF-001 -> HTTP 404 Not Found)
        var usuario = await _userRepository.GetByIdAsync(request.UsuarioId);
        if (usuario is null)
        {
            throw new NotFoundException(
                "NTF-001",
                "El usuario destinatario no fue encontrado.");
        }

        // 3. Crear la entidad Notification con estado "Enviada" y persistir en SQLite
        var notification = new Notification(
            request.UsuarioId,
            request.Mensaje.Trim(),
            request.Tipo.Trim(),
            "Enviada");

        await _notificationRepository.CreateAsync(notification);

        // 4. Retornar el DTO de respuesta
        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
    // Consultar historial de notificaciones de un usuario (NTF-001 y NTF-003)
    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        // 1. Verificar que el usuario exista en la base de datos (NTF-001 -> HTTP 404 Not Found)
        var usuario = await _userRepository.GetByIdAsync(userId);
        if (usuario is null)
        {
            throw new NotFoundException(
                "NTF-001",
                "El usuario destinatario no fue encontrado.");
        }

        // 2. Buscar las notificaciones registradas para ese usuario
        var notificaciones = (await _notificationRepository.GetByUserIdAsync(userId)).ToList();

        // 3. Si no tiene notificaciones registradas, devolver error NTF-003 (HTTP 404 Not Found)
        if (notificaciones.Count == 0)
        {
            throw new NotFoundException(
                "NTF-003",
                "No se encontraron notificaciones para el usuario.");
        }

        // 4. Mapear las entidades a DTOs de respuesta
        return notificaciones.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UsuarioId = n.UsuarioId,
            Mensaje = n.Mensaje,
            Tipo = n.Tipo,
            Estado = n.Estado,
            FechaEnvio = n.FechaEnvio
        });
    }
    private static void ValidarRequest(SendNotificationRequest? request)
    {
        if (request is null)
        {
            throw new ValidationException(
                "NTF-002",
                new[] { "Los datos de la notificación son inválidos." });
        }

        var errores = new List<string>();

        if (request.UsuarioId == Guid.Empty)
        {
            errores.Add("El campo 'usuarioId' es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            errores.Add("El campo 'mensaje' es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Tipo))
        {
            errores.Add("El campo 'tipo' es obligatorio.");
        }
        else if (!TiposPermitidos.Contains(request.Tipo.Trim(), StringComparer.Ordinal))
        {
            errores.Add("El tipo de notificación debe ser 'Email', 'Push' o 'SMS'.");
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("NTF-002", errores);
        }
    }
}