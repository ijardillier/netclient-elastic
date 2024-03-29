using Destructurama;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;

namespace NetClient.Elastic.Extensions
{
    public static class CustomExtensions
    {   
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            IHealthChecksBuilder hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            hcBuilder.ForwardToPrometheus();

            return services;
        }

        public static ILoggingBuilder AddSerilog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.UsingAttributes()
                .CreateLogger();

            builder.AddSerilog();

            return builder;
        }
    }
}
