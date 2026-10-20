using Microsoft.AspNetCore.Http;
using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints;

public static class UsersEndpointsExtension
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // Endpoint POST para registrar un usuario
        app.MapPost("/api/users/register", async (RegisterUserRequest peticion, UserService servicioUsuarios) =>
        {
            // Le pasamos el DTO de entrada a nuestro servicio 
            RegisterResponse respuesta = await servicioUsuarios.RegisterAsync(peticion);

            // Si todo salió bien, devolvemos HTTP 201 (Created) y el DTO de respuesta
            return Results.Created($"/api/users/{respuesta.Id}", respuesta);
        })
        .WithTags("Users"); // Esto agrupa el endpoint visualmente en Swagger
    }
}