using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        .WithTags("Notifications")
        .Produces<NotificationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Endpoint GET para consultar las notificaciones de un usuario
        app.MapGet("/api/notifications/{userId:guid}", async (Guid userId, NotificationService servicioNotificaciones) =>
        {
            IEnumerable<NotificationResponse> notificaciones = await servicioNotificaciones.GetByUserIdAsync(userId);
            return Results.Ok(notificaciones);
        })
        .WithTags("Notifications")
        .Produces<IEnumerable<NotificationResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}