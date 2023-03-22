using Autofac.Extensions.DependencyInjection;
using NetClient.Elastic.Extensions;
using Serilog;

namespace NetClient.Elastic
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await BuildHost(args).RunAsync();
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((host, builder) => builder.ClearProviders().AddSerilog(host.Configuration))
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build();
    }
}