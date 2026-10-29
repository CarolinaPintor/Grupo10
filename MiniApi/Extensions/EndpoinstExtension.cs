using MiniApi.Extensions.Endpoints;
using MiniApi.Models;

namespace MiniApi.Extensions;

public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapItemEndpoints();
        app.MapProductEndpoints();
        app.MapCartEndpoints();
        app.MapOrderEndpoints();

        // Se agregan los endpoints de usuario
        app.MapUserEndpoints();
        // Se agregan los endpoints de notificaciones
        app.MapNotificationEndpoints();
    }
}