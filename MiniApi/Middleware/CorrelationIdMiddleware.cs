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

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

            // Si no viene (o es demasiado largo) se genera uno nuevo
            if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > MaxLength)
                correlationId = Guid.NewGuid().ToString();

            context.Items[ItemKey] = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
