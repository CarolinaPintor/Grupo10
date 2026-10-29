using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniApi.Common;

public class ProductsSwaggerFilter : IOperationFilter
{
    private const string ProductoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.TrimEnd('/') ?? "";
        var method = context.ApiDescription.HttpMethod;

        if (path == "api/products" && method == "GET")
        {
            ConfigurarEjemplosObtenerTodos(operation);
        }
        else if (path == "api/products" && method == "POST")
        {
            ConfigurarEjemplosCrear(operation);
        }
        else if (path == "api/products/{id:guid}" && method == "GET")
        {
            ConfigurarEjemplosObtenerPorId(operation);
        }
        else if (path == "api/products/{id:guid}" && method == "PUT")
        {
            ConfigurarEjemplosActualizar(operation);
        }
        else if (path == "api/products/{id:guid}" && method == "DELETE")
        {
            ConfigurarEjemplosEliminar(operation);
        }
    }

    private static void ConfigurarEjemplosObtenerTodos(OpenApiOperation operation)
    {
        operation.Summary = "Obtener todos los productos";
        operation.Description = "Devuelve el listado completo de productos registrados. Permite filtrar opcionalmente por categoría y nombre.";

        SetResponseExample(operation, "200", "application/json", """
        [
          {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "nombre": "Notebook Dell XPS 15",
            "descripcion": "Laptop 15 pulgadas, 32GB RAM",
            "precio": 1500.00,
            "stock": 10,
            "categoria": "Electrónica",
            "fechaCreacion": "2024-01-15T10:30:00Z"
          }
        ]
        """);

        SetResponseExample(operation, "500", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/products",
          "errorCode": "PRD-005",
          "errorMessage": "Error interno al procesar el producto."
        }
        """);
    }

    private static void ConfigurarEjemplosCrear(OpenApiOperation operation)
    {
        operation.Summary = "Crear un nuevo producto";
        operation.Description = "Valida los datos (PRD-002), comprueba que no exista otro producto con el mismo nombre en la categoría (PRD-003) y registra el producto.";

        SetRequestExample(operation, """
        {
          "nombre": "Notebook Dell XPS 15",
          "descripcion": "Laptop 15 pulgadas, 32GB RAM",
          "precio": 1500.00,
          "stock": 10,
          "categoria": "Electrónica"
        }
        """);

        SetResponseExample(operation, "201", "application/json", """
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "nombre": "Notebook Dell XPS 15",
          "descripcion": "Laptop 15 pulgadas, 32GB RAM",
          "precio": 1500.00,
          "stock": 10,
          "categoria": "Electrónica",
          "fechaCreacion": "2024-01-15T10:30:00Z"
        }
        """);

        SetResponseExample(operation, "400", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          "title": "Bad Request",
          "status": 400,
          "detail": "Los datos proporcionados no son válidos.",
          "instance": "/api/products",
          "errorCode": "PRD-002",
          "errorMessage": "Los datos del producto son inválidos."
        }
        """);

        SetResponseExample(operation, "409", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
          "title": "Conflict",
          "status": 409,
          "detail": "Ya existe un recurso con esos datos.",
          "instance": "/api/products",
          "errorCode": "PRD-003",
          "errorMessage": "Ya existe un producto con ese nombre en la categoría 'Electrónica'."
        }
        """);

        SetResponseExample(operation, "500", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/products",
          "errorCode": "PRD-005",
          "errorMessage": "Error interno al procesar el producto."
        }
        """);
    }

    private static void ConfigurarEjemplosObtenerPorId(OpenApiOperation operation)
    {
        operation.Summary = "Obtener un producto por ID";
        operation.Description = "Devuelve los datos del producto identificado por el GUID indicado.";

        SetResponseExample(operation, "200", "application/json", """
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "nombre": "Notebook Dell XPS 15",
          "descripcion": "Laptop 15 pulgadas, 32GB RAM",
          "precio": 1500.00,
          "stock": 10,
          "categoria": "Electrónica",
          "fechaCreacion": "2024-01-15T10:30:00Z"
        }
        """);

        SetResponseExample(operation, "404", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
          "title": "Not Found",
          "status": 404,
          "detail": "El recurso solicitado no fue encontrado.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-001",
          "errorMessage": "Producto no encontrado."
        }
        """);

        SetResponseExample(operation, "500", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-005",
          "errorMessage": "Error interno al procesar el producto."
        }
        """);
    }

    private static void ConfigurarEjemplosActualizar(OpenApiOperation operation)
    {
        operation.Summary = "Actualizar un producto existente";
        operation.Description = "Actualiza los datos del producto indicado por su GUID.";

        SetRequestExample(operation, """
        {
          "nombre": "Notebook Dell XPS 15",
          "descripcion": "Laptop 15 pulgadas, 64GB RAM",
          "precio": 1750.00,
          "stock": 8,
          "categoria": "Electrónica"
        }
        """);

        SetResponseExample(operation, "200", "application/json", """
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "nombre": "Notebook Dell XPS 15",
          "descripcion": "Laptop 15 pulgadas, 64GB RAM",
          "precio": 1750.00,
          "stock": 8,
          "categoria": "Electrónica",
          "fechaCreacion": "2024-01-15T10:30:00Z"
        }
        """);

        SetResponseExample(operation, "400", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          "title": "Bad Request",
          "status": 400,
          "detail": "Los datos proporcionados no son válidos.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-002",
          "errorMessage": "Los datos del producto son inválidos."
        }
        """);

        SetResponseExample(operation, "404", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
          "title": "Not Found",
          "status": 404,
          "detail": "El recurso solicitado no fue encontrado.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-001",
          "errorMessage": "Producto no encontrado."
        }
        """);

        SetResponseExample(operation, "500", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-005",
          "errorMessage": "Error interno al procesar el producto."
        }
        """);
    }

    private static void ConfigurarEjemplosEliminar(OpenApiOperation operation)
    {
        operation.Summary = "Eliminar un producto existente";
        operation.Description = "Elimina el producto indicado por su GUID si existe.";

        SetResponseExample(operation, "404", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
          "title": "Not Found",
          "status": 404,
          "detail": "El recurso solicitado no fue encontrado.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-001",
          "errorMessage": "Producto no encontrado."
        }
        """);

        SetResponseExample(operation, "409", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
          "title": "Conflict",
          "status": 409,
          "detail": "No se puede eliminar el recurso.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-004",
          "errorMessage": "El producto tiene órdenes activas y no puede eliminarse."
        }
        """);

        SetResponseExample(operation, "500", "application/problem+json", """
        {
          "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
          "title": "Internal Server Error",
          "status": 500,
          "detail": "Ocurrió un error interno.",
          "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "errorCode": "PRD-005",
          "errorMessage": "Error interno al procesar el producto."
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