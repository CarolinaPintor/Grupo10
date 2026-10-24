using Microsoft.AspNetCore.Http;
using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class NotificationsEndpointsExtension
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        // Endpoint POST para enviar/registrar una notificación
        app.MapPost("/api/notifications/send", async (SendNotificationRequest peticion, NotificationService servicioNotificaciones) =>
        {
            NotificationResponse respuesta = await servicioNotificaciones.SendAsync(peticion);
            return Results.Created($"/api/notifications/{respuesta.Id}", respuesta);
        })
        .WithTags("Notifications");

        // Endpoint GET para consultar las notificaciones de un usuario
        app.MapGet("/api/notifications/{userId:guid}", async (Guid userId, NotificationService servicioNotificaciones) =>
        {
            IEnumerable<NotificationResponse> notificaciones = await servicioNotificaciones.GetByUserIdAsync(userId);
            return Results.Ok(notificaciones);
        })
        .WithTags("Notifications");
    }
}