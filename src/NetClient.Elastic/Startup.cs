using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NetClient.Elastic.Extensions;
using NetClient.Elastic.Services;
using NetClient.Elastic.Tasks;
using Prometheus;
using Refit;
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
            services.Configure<Settings>(Configuration);
            services.AddOptions();
            services.AddCustomHealthCheck(Configuration);
            
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient();

            services.AddHostedService<DataService>();        
            services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri($"{Configuration["BaseAddress"]}")
            });

            services.AddRefitClient<IPersonApiService>().ConfigureHttpClient(x =>
            {
                x.BaseAddress = new Uri($"{Configuration["PersonApiBaseAddress"]}");
            });

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
        /// <param name="env">The host environment.</param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseAllElasticApm(Configuration);            
            
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseStaticFiles();
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

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}