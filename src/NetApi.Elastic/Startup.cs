using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NetApi.Elastic.Extensions;
using Prometheus;
using Serilog;

namespace NetApi.Elastic
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
                .AddOptions()
                .AddCustomHealthCheck(Configuration);


            services.AddApiVersioning()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddCors(policy =>
                {
                    policy.AddPolicy("OpenCorsPolicy", opt => opt
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
                });

            services.AddControllers();   

            // Suppress default metrics
            Metrics.SuppressDefaultMetrics();

            // Defines statics labels for metrics
            Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
            {
                { "domain", "NetApi" },
                { "domain_context", "NetApi.Elastic" }
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline. Automatically called by the runtime.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseAllElasticApm(Configuration);            
            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseCors("OpenCorsPolicy");

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

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );
            });                     
        }
    }
}




