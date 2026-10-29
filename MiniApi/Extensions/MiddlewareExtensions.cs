using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MiniApi.Middleware;
using Serilog;

namespace MiniApi.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // Primero de todos, para que el resto del pipeline ya tenga el CorrelationId
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLogContextMiddleware>();

            // Swagger disponible en todos los entornos: el enunciado pide /swagger en cada servicio
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex != null) ? Serilog.Events.LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;
            });

            app.UseMiddleware<AuditMiddleware>();

            // Va después de Audit para que la respuesta de error también quede auditada
            app.UseExceptionHandler();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            // Liveness: la app está viva. No ejecuta ningún check, solo confirma que responde
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Readiness: la app puede atender requests. Ejecuta solo los checks de la base de datos
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("database"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}