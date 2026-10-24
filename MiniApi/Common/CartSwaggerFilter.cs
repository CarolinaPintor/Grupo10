using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniApi.Common;

// Agrega ejemplos de request y response (éxito y error) a los endpoints de /api/cart.
// Sigue el mismo patrón que UsersSwaggerFilter.
public class CartSwaggerFilter : IOperationFilter
{
    private const string UsuarioId = "a1b2c3d4-0000-0000-0000-111122223333";
    private const string ProductoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // La ruta viene con el constraint ({userId:guid}); se saca para comparar con {userId}
        var path = Regex.Replace(context.ApiDescription.RelativePath?.TrimEnd('/') ?? "", @"\{(\w+):[^}]+\}", "{$1}");
        var method = context.ApiDescription.HttpMethod;

        var esEndpointDeCart = true;

        if (path == "api/cart/{userId}" && method == "GET")
        {
            ConfigurarEjemplosObtener(operation);
        }
        else if (path == "api/cart/{userId}/items" && method == "POST")
        {
            ConfigurarEjemplosAgregar(operation);
        }
        else if (path == "api/cart/{userId}/items/{productId}" && method == "PUT")
        {
            ConfigurarEjemplosActualizar(operation);
        }
        else if (path == "api/cart/{userId}/items/{productId}" && method == "DELETE")
        {
            ConfigurarEjemplosQuitarItem(operation);
        }
        else if (path == "api/cart/{userId}" && method == "DELETE")
        {
            ConfigurarEjemplosVaciar(operation);
        }
        else
        {
            esEndpointDeCart = false;
        }

        // Todos los endpoints de Cart pueden responder 500 (CRT-005)
        if (esEndpointDeCart)
        {
            var instance = "/" + path.Replace("{userId}", UsuarioId).Replace("{productId}", ProductoId);
            SetResponseExample(operation, "500", "application/problem+json",
                Problema(500, instance, "CRT-005", "Error interno al procesar el carrito."));
        }
    }

    private static void ConfigurarEjemplosObtener(OpenApiOperation operation)
    {
        SetResponseExample(operation, "200", "application/json", Carrito(2));
        SetResponseExample(operation, "404", "application/problem+json",
            Problema(404, $"/api/cart/{UsuarioId}", "CRT-001", CarritoInexistente));
    }

    private static void ConfigurarEjemplosAgregar(OpenApiOperation operation)
    {
        var instance = $"/api/cart/{UsuarioId}/items";

        SetRequestExample(operation, $$"""
        {
          "productoId": "{{ProductoId}}",
          "cantidad": 2
        }
        """);
        SetResponseExample(operation, "200", "application/json", Carrito(2));
        SetResponseExample(operation, "400", "application/problem+json",
            Problema(400, instance, "CRT-004", "La cantidad debe ser mayor a cero."));
        SetResponseExample(operation, "404", "application/problem+json",
            Problema(404, instance, "CRT-002", $"No se encontró el producto con ID '{ProductoId}'."));
        SetResponseExample(operation, "422", "application/problem+json",
            Problema(422, instance, "CRT-003", "Stock insuficiente. Disponible: 1, solicitado: 5."));
    }

    private static void ConfigurarEjemplosActualizar(OpenApiOperation operation)
    {
        var instance = $"/api/cart/{UsuarioId}/items/{ProductoId}";

        SetRequestExample(operation, """
        {
          "cantidad": 4
        }
        """);
        SetResponseExample(operation, "200", "application/json", Carrito(4));
        SetResponseExample(operation, "400", "application/problem+json",
            Problema(400, instance, "CRT-004", "La cantidad debe ser mayor a cero."));
        SetResponseExamples(operation, "404", "Carrito inexistente (CRT-001) o producto no encontrado (CRT-002).",
            new Dictionary<string, (string Resumen, string Json)>
            {
                ["CRT-001 (Carrito inexistente)"] = ("El usuario no tiene carrito",
                    Problema(404, instance, "CRT-001", CarritoInexistente)),
                ["CRT-002 (Producto no encontrado)"] = ("El producto no existe o no está en el carrito",
                    Problema(404, instance, "CRT-002", ProductoFueraDelCarrito))
            });
        SetResponseExample(operation, "422", "application/problem+json",
            Problema(422, instance, "CRT-003", "Stock insuficiente. Disponible: 1, solicitado: 5."));
    }

    private static void ConfigurarEjemplosQuitarItem(OpenApiOperation operation)
    {
        var instance = $"/api/cart/{UsuarioId}/items/{ProductoId}";

        SetResponseExamples(operation, "404", "Carrito inexistente (CRT-001) o producto fuera del carrito (CRT-002).",
            new Dictionary<string, (string Resumen, string Json)>
            {
                ["CRT-001 (Carrito inexistente)"] = ("El usuario no tiene carrito",
                    Problema(404, instance, "CRT-001", CarritoInexistente)),
                ["CRT-002 (Producto fuera del carrito)"] = ("El producto no estaba en el carrito",
                    Problema(404, instance, "CRT-002", ProductoFueraDelCarrito))
            });
    }

    private static void ConfigurarEjemplosVaciar(OpenApiOperation operation)
    {
        SetResponseExample(operation, "404", "application/problem+json",
            Problema(404, $"/api/cart/{UsuarioId}", "CRT-001", CarritoInexistente));
    }

    private static string CarritoInexistente =>
        $"El usuario '{UsuarioId}' no tiene un carrito activo.";

    private static string ProductoFueraDelCarrito =>
        $"El producto '{ProductoId}' no está en el carrito.";

    // Carrito con un solo producto, igual al que devuelve la API
    private static string Carrito(int cantidad) => $$"""
        {
          "usuarioId": "{{UsuarioId}}",
          "items": [
            {
              "productoId": "{{ProductoId}}",
              "cantidad": {{cantidad}}
            }
          ],
          "fechaActualizacion": "2026-10-23T10:45:00Z"
        }
        """;

    // Respuesta de error con el mismo formato que arma ErrorResponseWriter
    private static string Problema(int status, string instance, string errorCode, string errorMessage)
    {
        var (type, title, detail) = status switch
        {
            400 => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud no es válida."),
            404 => ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
            422 => ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error interno.")
        };

        return $$"""
        {
          "type": "{{type}}",
          "title": "{{title}}",
          "status": {{status}},
          "detail": "{{detail}}",
          "instance": "{{instance}}",
          "errorCode": "{{errorCode}}",
          "errorMessage": "{{errorMessage}}"
        }
        """;
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

    // Varios ejemplos para una misma respuesta (aparecen en un selector desplegable)
    private static void SetResponseExamples(
        OpenApiOperation operation, string statusCode, string descripcion,
        Dictionary<string, (string Resumen, string Json)> ejemplos)
    {
        if (operation.Responses != null &&
            operation.Responses.TryGetValue(statusCode, out var response) &&
            response is OpenApiResponse concreteResponse)
        {
            concreteResponse.Description = descripcion;
            concreteResponse.Content ??= new Dictionary<string, OpenApiMediaType>();

            var examples = new Dictionary<string, IOpenApiExample>();
            foreach (var ejemplo in ejemplos)
            {
                examples[ejemplo.Key] = new OpenApiExample
                {
                    Summary = ejemplo.Value.Resumen,
                    Value = JsonNode.Parse(ejemplo.Value.Json)
                };
            }

            concreteResponse.Content["application/problem+json"] = new OpenApiMediaType { Examples = examples };
        }
    }
}
