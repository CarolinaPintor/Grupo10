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

        // Endpoint POST para iniciar sesión (C07)
        app.MapPost("/api/users/login", async (LoginRequest peticion, UserService servicioUsuarios) =>
        {
            // 1. Le pasamos el DTO de entrada al servicio, que es el "cerebro" de la operación (C06)
            LoginResponse respuesta = await servicioUsuarios.LoginAsync(peticion);

            // 2. Si el servicio no lanzó ninguna excepción, significa que la contraseña era correcta.
            // Devolvemos un código HTTP 200 (OK) junto con el DTO seguro (que no tiene la contraseña).
            return Results.Ok(respuesta);
        })
        .WithTags("Users"); // Agrupa este endpoint en la misma sección "Users" de Swagger
    }
}