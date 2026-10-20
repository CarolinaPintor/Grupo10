using MiniApi.Extensions.Endpoints;
using MiniApi.Models;

namespace MiniApi.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
          
        app.MapItemEndpoints();
        // se agregan los endpoints de usuario
        app.MapUserEndpoints();
    }
}