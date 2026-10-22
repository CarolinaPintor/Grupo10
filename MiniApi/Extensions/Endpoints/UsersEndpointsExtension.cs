using Microsoft.AspNetCore.Http;
using MiniApi.DTOs;
using MiniApi.Services;

namespace MiniApi.Extensions.Endpoints
{
    public static class UsersEndpointsExtension
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            // Endpoint POST para registrar un usuario (C05 + C08)
            app.MapPost("/api/users/register", async (RegisterUserRequest peticion, UserService servicioUsuarios) =>
            {
                RegisterResponse respuesta = await servicioUsuarios.RegisterAsync(peticion);
                return Results.Created($"/api/users/{respuesta.Id}", respuesta);
            })
            .WithTags("Users")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

            // Endpoint POST para iniciar sesión (C07 + C08)
            app.MapPost("/api/users/login", async (LoginRequest peticion, UserService servicioUsuarios) =>
            {
                LoginResponse respuesta = await servicioUsuarios.LoginAsync(peticion);
                return Results.Ok(respuesta);
            })
            .WithTags("Users")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        }
    }
}   