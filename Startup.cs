// using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NetClient.Elastic.Extensions;
using NetClient.Elastic.Tasks;
using Prometheus;
using Serilog;

namespace NetClient.Elastic
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Adds all custom configurations for this service.
            services
                .Configure<Settings>(Configuration)
                .AddOptions()
                .AddCustomHealthCheck(Configuration)
                .AddHostedService<DataService>();        

            // Suppress default metrics
            Metrics.SuppressDefaultMetrics();

            // Defines statics labels for metrics
            Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
            {
                { "domain", "NetClient" },
                { "domain_context", "NetClient.Elastic" }
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline. Automatically called by the runtime.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            //app.UseAllElasticApm(Configuration);            
            app.UseRouting();
            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapMetrics();
            });
        }
    }
}