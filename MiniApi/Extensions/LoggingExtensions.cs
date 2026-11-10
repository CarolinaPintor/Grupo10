using MiniApi.Middleware;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace MiniApi.Extensions
{
    public static class LoggingExtensions
    {
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Servicio", "MiniApi")

                // ALUMNO: LOG DE CONSOLA: libre para usarse como quieran
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Information)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))

                // ALUMNO: LOG DE CONSOLA: Solo registra cuando se consume un endpoint, es una auditoría limpia. 
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        // Solo aceptar logs que vengan del Middleware de Serilog
                        // Esto elimina automáticamente los duplicados de Microsoft.AspNetCore.Mvc
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                        // Excluir rutas irrelevantes
                        if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                            pathValue is ScalarValue scalar && scalar.Value is string path)
                        {
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
                        }

                        return true;
                    })
                    .WriteTo.File(
                        path: "logs/audit.log",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {RequestMethod} | {RequestPath} | {StatusCode} | {CorrelationId}{NewLine}",
                        rollingInterval: RollingInterval.Day))

                // LOG JSON: formato estructurado con Timestamp, Level, Servicio, Endpoint, CorrelationId y ErrorCode.
                // Se excluye AuditMiddleware porque registra el body de los requests (incluye contraseñas).
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(Matching.FromSource<AuditMiddleware>())
                    .Filter.ByExcluding(Matching.FromSource("System.Net.Http")) // HealthChecks UI consultando /health
                    .Filter.ByExcluding(le =>
                        le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                        pathValue is ScalarValue scalar && scalar.Value is string path &&
                        (path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
                         path.Contains("/swagger", StringComparison.OrdinalIgnoreCase)))
                    .WriteTo.File(
                        formatter: new JsonFormatter(renderMessage: true),
                        path: "logs/app.json",
                        rollingInterval: RollingInterval.Day))
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}