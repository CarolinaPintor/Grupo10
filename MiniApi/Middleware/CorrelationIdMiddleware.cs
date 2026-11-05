using Serilog.Context;

namespace MiniApi.Middleware
{
    // Asigna un X-Correlation-Id a cada request: usa el que llega en el header o genera uno nuevo.
    // Queda en HttpContext.Items y se devuelve en el header de la respuesta.
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string ItemKey = "CorrelationId";
        private const int MaxLength = 100;

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        // Se ejecuta justo antes de enviar la respuesta y le agrega el header
        private static Task AgregarHeaderCorrelationId(object estado)
        {
            var context = (HttpContext)estado;
            context.Response.Headers[HeaderName] = context.Items[ItemKey]?.ToString();
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName].ToString();

            // Si no viene (o es demasiado largo) se genera uno nuevo
            if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > MaxLength)
                correlationId = Guid.NewGuid().ToString();

            context.Items[ItemKey] = correlationId;
            context.Response.OnStarting(AgregarHeaderCorrelationId, context);

            // Todos los logs del request llevan el CorrelationId
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
