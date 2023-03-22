using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace NetApi.Elastic.Extensions
{
    public static class CustomExtensions
    {   
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            IHealthChecksBuilder hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            return services;
        }

        public static ILoggingBuilder AddSerilog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            builder.AddSerilog();

            return builder;
        }
    }
}
