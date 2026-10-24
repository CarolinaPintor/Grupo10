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
    }
}