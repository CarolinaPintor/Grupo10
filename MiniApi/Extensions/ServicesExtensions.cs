using Microsoft.AspNetCore.Http;
using MiniApi.Data;
using MiniApi.ExceptionHandlers;
using MiniApi.HealthChecks;
using MiniApi.Services;
using MiniApi.Common;

namespace MiniApi.Extensions
{
    public static class ServicesExtensions
    {
        private static void ConfigurarRutas(RouteHandlerOptions opciones)
        {
            opciones.ThrowOnBadRequest = true;
        }

        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<CartRepository>();
            services.AddScoped<CartService>();
            services.AddScoped<ProductRepository>();
            services.AddScoped<ProductService>();
            //agrego el repositorio de usuarios
            services.AddScoped<UserRepository>();
            //agrego el servicio de usuarios
            services.AddScoped<UserService>();
            // Agrego el repositorio de notificaciones
            services.AddScoped<NotificationRepository>();
            services.AddScoped<NotificationService>();
            // agrego el repositorio de órdenes
            services.AddScoped<OrderRepository>();
            services.AddScoped<OrderService>();

            // Orden: los más específicos primero, el global al final
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();

            services.AddExceptionHandler<UnauthorizedExceptionHandler>();
            services.AddExceptionHandler<ForbiddenExceptionHandler>();
            services.AddExceptionHandler<BadRequestExceptionHandler>();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Sin esto, un JSON malformado solo llega al handler de errores en Development
            services.Configure<RouteHandlerOptions>(ConfigurarRutas);

            services.AddEndpointsApiExplorer();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<UsersSwaggerFilter>();
                options.OperationFilter<ProductsSwaggerFilter>();
                options.OperationFilter<CartSwaggerFilter>();
                options.OperationFilter<OrdersSwaggerFilter>();
            });

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