using MiniApi.Data;
using MiniApi.ExceptionHandlers;
using MiniApi.HealthChecks;
using MiniApi.Services;

namespace MiniApi.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<ItemRepository>();
            //agrego el repositorio de usuarios
            services.AddScoped<UserRepository>();
            //agrego el servicio de usuarios
            services.AddScoped<UserService>();

            // Orden: los más específicos primero, el global al final
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();

            services.AddExceptionHandler<UnauthorizedExceptionHandler>();
            services.AddExceptionHandler<ForbiddenExceptionHandler>();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();
        }
    }
}