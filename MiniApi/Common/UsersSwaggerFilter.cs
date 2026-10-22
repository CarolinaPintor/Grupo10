using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniApi.Common;

public class UsersSwaggerFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.TrimEnd('/');

        if (path == "api/users/register")
        {
            ConfigurarEjemplosRegistro(operation);
        }
        else if (path == "api/users/login")
        {
            ConfigurarEjemplosLogin(operation);
        }
    }

    private static void ConfigurarEjemplosRegistro(OpenApiOperation operation)
    {
        operation.Summary = "Registra un nuevo usuario en el sistema";
        operation.Description = "Crea una cuenta de usuario validando formato de datos (USR-002) y unicidad de email (USR-001).";

        // 1. Ejemplo de Request (Solicitud)
        SetRequestExample(operation, """
        {
          "nombre": "Ana",
          "apellido": "García",
          "email": "ana.garcia@ejemplo.com",
          "password": "Password123"
        }
        """);

        // 2. Ejemplo 201 Created
        SetResponseExample(operation, "201", "application/json", """
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "nombre": "Ana",
          "apellido": "García",
          "email": "ana.garcia@ejemplo.com",
          "fechaRegistro": "2026-10-21T18:30:00Z",
          "activo": true
        }
        """);

        // 3. Ejemplo 400 Bad Request (USR-002)
        SetResponseExample(operation, "400", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          "title": "Bad Request",
          "status": 400,
          "detail": "Los datos proporcionados no son válidos.",
          "instance": "/api/users/register",
          "errorCode": "USR-002",
          "errorMessage": "El formato del email no es válido.",
          "errors": [
            "El formato del email no es válido.",
            "La contraseña debe tener al menos 6 caracteres."
          ]
        }
        """);

        // 4. Ejemplo 409 Conflict (USR-001)
        SetResponseExample(operation, "409", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
          "title": "Conflict",
          "status": 409,
          "detail": "Conflicto con el estado actual del recurso.",
          "instance": "/api/users/register",
          "errorCode": "USR-001",
          "errorMessage": "El email ya está registrado."
        }
        """);
    }

    private static void ConfigurarEjemplosLogin(OpenApiOperation operation)
    {
        operation.Summary = "Autentica a un usuario existente";
        operation.Description = "Verifica las credenciales del usuario. Bloquea la cuenta tras 3 intentos fallidos consecutivos.";

        // 1. Ejemplo de Request (Solicitud)
        SetRequestExample(operation, """
        {
          "email": "ana.garcia@ejemplo.com",
          "password": "Password123"
        }
        """);

        // 2. Ejemplo 200 OK
        SetResponseExample(operation, "200", "application/json", """
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "nombre": "Ana",
          "apellido": "García",
          "email": "ana.garcia@ejemplo.com"
        }
        """);

        // 3. Ejemplo 401 Unauthorized (USR-003)
        SetResponseExample(operation, "401", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
          "title": "Unauthorized",
          "status": 401,
          "detail": "Las credenciales no son válidas.",
          "instance": "/api/users/login",
          "errorCode": "USR-003",
          "errorMessage": "Credenciales incorrectas."
        }
        """);

        // 4. Ejemplos 403 Forbidden (USR-004 y USR-005 en un selector desplegable)
        if (operation.Responses != null &&
            operation.Responses.TryGetValue("403", out var response403) &&
            response403 is OpenApiResponse concrete403)
        {
            concrete403.Description = "Cuenta bloqueada por intentos fallidos (USR-004) o suspendida manualmente (USR-005).";
            concrete403.Content ??= new Dictionary<string, OpenApiMediaType>();
            concrete403.Content["application/problem+json"] = new OpenApiMediaType
            {
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    ["USR-004 (Bloqueo por 3 intentos)"] = new OpenApiExample
                    {
                        Summary = "Cuenta bloqueada por fuerza bruta",
                        Value = JsonNode.Parse("""
                        {
                          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                          "title": "Forbidden",
                          "status": 403,
                          "detail": "El acceso está prohibido.",
                          "instance": "/api/users/login",
                          "errorCode": "USR-004",
                          "errorMessage": "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
                        }
                        """)
                    },
                    ["USR-005 (Cuenta suspendida)"] = new OpenApiExample
                    {
                        Summary = "Cuenta suspendida por un administrador",
                        Value = JsonNode.Parse("""
                        {
                          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                          "title": "Forbidden",
                          "status": 403,
                          "detail": "El acceso está prohibido.",
                          "instance": "/api/users/login",
                          "errorCode": "USR-005",
                          "errorMessage": "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte."
                        }
                        """)
                    }
                }
            };
        }
    }

    private static void SetRequestExample(OpenApiOperation operation, string json)
    {
        if (operation.RequestBody?.Content != null &&
            operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = JsonNode.Parse(json);
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, string contentType, string json)
    {
        if (operation.Responses != null &&
            operation.Responses.TryGetValue(statusCode, out var response) &&
            response is OpenApiResponse concreteResponse)
        {
            concreteResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
            concreteResponse.Content[contentType] = new OpenApiMediaType
            {
                Example = JsonNode.Parse(json)
            };
        }
    }
}