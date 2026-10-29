using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniApi.Common;

public class OrdersSwaggerFilter : IOperationFilter
{
    private const string OrderId = "f1e2d3c4-0000-0000-0000-aabbccddeeff";
    private const string UserId = "a1b2c3d4-0000-0000-0000-111122223333";
    private const string ProductId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.TrimEnd('/');
        var method = context.ApiDescription.HttpMethod;

        if (path == "api/orders")
        {
            if (method == "GET")
            {
                ConfigurarEjemplosListar(operation);
            }
            else if (method == "POST")
            {
                ConfigurarEjemplosCrear(operation);
            }
        }
        else if (path != null && path.EndsWith("/status"))
        {
            ConfigurarEjemplosActualizarEstado(operation);
        }
        else if (path != null && path.StartsWith("api/orders/"))
        {
            ConfigurarEjemplosObtenerPorId(operation);
        }
    }

    private static void ConfigurarEjemplosListar(OpenApiOperation operation)
    {
        operation.Summary = "Listar órdenes";
        operation.Description = "Devuelve el listado de órdenes registradas. Permite filtrar opcionalmente por ?usuarioId=";

        // 1. Ejemplo 200 OK
        SetResponseExample(operation, "200", "application/json", $$"""
        [
          {
            "id": "{{OrderId}}",
            "usuarioId": "{{UserId}}",
            "items": [
              {
                "productoId": "{{ProductId}}",
                "cantidad": 2,
                "precioUnitario": 1500.00
              }
            ],
            "total": 3000.00,
            "estado": "Pendiente",
            "fechaCreacion": "2024-03-10T11:00:00Z"
          }
        ]
        """);

        // 2. Ejemplo 500 Internal Server Error (ORD-007)
        SetResponseExample(operation, "500", "application/problem+json", $$"""
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/orders",
          "errorCode": "ORD-007",
          "errorMessage": "Error interno al procesar la orden."
        }
        """);
    }

    private static void ConfigurarEjemplosCrear(OpenApiOperation operation)
    {
        operation.Summary = "Crear nueva orden";
        operation.Description = "Crea una nueva orden validando datos (ORD-002), usuario (ORD-003), productos (ORD-004) y stock (ORD-005).";

        // 1. Ejemplo de Request
        SetRequestExample(operation, $$"""
        {
          "usuarioId": "{{UserId}}",
          "items": [
            {
              "productoId": "{{ProductId}}",
              "cantidad": 2
            }
          ]
        }
        """);

        // 2. Ejemplo 201 Created
        SetResponseExample(operation, "201", "application/json", $$"""
        {
          "id": "{{OrderId}}",
          "usuarioId": "{{UserId}}",
          "items": [
            {
              "productoId": "{{ProductId}}",
              "cantidad": 2,
              "precioUnitario": 1500.00
            }
          ],
          "total": 3000.00,
          "estado": "Pendiente",
          "fechaCreacion": "2024-03-10T11:00:00Z"
        }
        """);

        // 3. Ejemplo 400 Bad Request (ORD-002)
        SetResponseExample(operation, "400", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          "title": "Bad Request",
          "status": 400,
          "detail": "Los datos proporcionados no son válidos.",
          "instance": "/api/orders",
          "errorCode": "ORD-002",
          "errorMessage": "Los datos de la orden son inválidos."
        }
        """);

        // 4. Ejemplo 404 Not Found (ORD-003 / ORD-004)
        SetResponseExample(operation, "404", "application/problem+json", $$"""
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
          "title": "Not Found",
          "status": 404,
          "detail": "El recurso solicitado no fue encontrado.",
          "instance": "/api/orders",
          "errorCode": "ORD-003",
          "errorMessage": "Usuario no encontrado al crear la orden."
        }
        """);

        // 5. Ejemplo 422 Unprocessable Entity (ORD-005)
        SetResponseExample(operation, "422", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
          "title": "Unprocessable Entity",
          "status": 422,
          "detail": "No se puede procesar la solicitud.",
          "instance": "/api/orders",
          "errorCode": "ORD-005",
          "errorMessage": "Stock insuficiente para 'Notebook Dell XPS 15'. Disponible: 2, solicitado: 5."
        }
        """);
    }

    private static void ConfigurarEjemplosObtenerPorId(OpenApiOperation operation)
    {
        operation.Summary = "Obtener detalle de una orden";
        operation.Description = "Devuelve los datos de la orden identificada por el GUID indicado, validando existencia (ORD-001).";

        // 1. Ejemplo 200 OK
        SetResponseExample(operation, "200", "application/json", $$"""
        {
          "id": "{{OrderId}}",
          "usuarioId": "{{UserId}}",
          "items": [
            {
              "productoId": "{{ProductId}}",
              "cantidad": 2,
              "precioUnitario": 1500.00
            }
          ],
          "total": 3000.00,
          "estado": "Pendiente",
          "fechaCreacion": "2024-03-10T11:00:00Z"
        }
        """);

        // 2. Ejemplo 404 Not Found (ORD-001)
        SetResponseExample(operation, "404", "application/problem+json", $$"""
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
          "title": "Not Found",
          "status": 404,
          "detail": "El recurso solicitado no fue encontrado.",
          "instance": "/api/orders/{{OrderId}}",
          "errorCode": "ORD-001",
          "errorMessage": "Orden no encontrada."
        }
        """);
    }

    private static void ConfigurarEjemplosActualizarEstado(OpenApiOperation operation)
    {
        operation.Summary = "Actualizar estado de la orden";
        operation.Description = "Modifica el estado de la orden validando existencia (ORD-001) y transiciones válidas (ORD-006).";

        // 1. Ejemplo de Request
        SetRequestExample(operation, """
        {
          "estado": "Confirmada"
        }
        """);

        // 2. Ejemplo 200 OK
        SetResponseExample(operation, "200", "application/json", $$"""
        {
          "id": "{{OrderId}}",
          "estado": "Confirmada",
          "fechaActualizacion": "2024-03-10T12:00:00Z"
        }
        """);

        // 3. Ejemplo 409 Conflict (ORD-006)
        SetResponseExample(operation, "409", "application/problem+json", $$"""
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
          "title": "Conflict",
          "status": 409,
          "detail": "No se puede modificar el estado.",
          "instance": "/api/orders/{{OrderId}}/status",
          "errorCode": "ORD-006",
          "errorMessage": "Una orden en estado 'Entregada' no puede volver a 'Pendiente'."
        }
        """);
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