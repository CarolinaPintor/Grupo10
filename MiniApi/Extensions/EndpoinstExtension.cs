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

        // Se agregan los endpoints de usuario
        app.MapUserEndpoints();
    }
}