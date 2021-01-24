using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreClient.Elk.Tasks
{
    public class DataService : BackgroundService
    {
        private readonly ILogger<DataService> _logger;
        private readonly Settings _settings;

        private readonly MetricServer _metricServer = new MetricServer(port: 9091);
        private readonly Random _random = new Random();
        private static readonly Gauge Gauge1 = Metrics.CreateGauge("myapp_gauge1_value", "A simple gauge 1");
        private static readonly Gauge Gauge2 = Metrics.CreateGauge("myapp_gauge2_value", "A simple gauge 2");
        private static readonly Gauge Gauge3 = Metrics.CreateGauge("myapp_gauge3_value", "A simple gauge 3");
        private static readonly Gauge Gauge4 = Metrics.CreateGauge("myapp_gauge4_value", "A simple gauge 4");
        private static readonly Gauge Gauge5 = Metrics.CreateGauge("myapp_gauge5_value", "A simple gauge 5");
        private static readonly Gauge Gauge6 = Metrics.CreateGauge("myapp_gauge6_value", "A simple gauge 6");
        private static readonly Gauge Gauge7 = Metrics.CreateGauge("myapp_gauge7_value", "A simple gauge 7");
        private static readonly Gauge Gauge8 = Metrics.CreateGauge("myapp_gauge8_value", "A simple gauge 8");
        private static readonly Gauge Gauge9 = Metrics.CreateGauge("myapp_gauge9_value", "A simple gauge 9");
  
        public DataService(IOptions<Settings> settings, ILogger<DataService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _metricServer.Start();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _metricServer.StopAsync();
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("{Source} background task is starting.", nameof(DataService));

            cancellationToken.Register(() => _logger.LogDebug("#1 {Source} background task is stopping.", nameof(DataService)));

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Source} background task is doing background work.", nameof(DataService));

                SendData();

                await Task.Delay(_settings.CheckConfigurationUpdateDelay, cancellationToken);
            }

            _logger.LogDebug("{Source} background task is stopping.", nameof(DataService));

            await Task.CompletedTask;
        }

        private void SendData()
        {
            _logger.LogDebug("{Source} is sending data.", nameof(DataService));

            Gauge1.Set(_random.Next(1000, 1500));
            Gauge2.Set(_random.Next(2000, 2500));
            Gauge3.Set(_random.Next(3000, 3500));
            Gauge4.Set(_random.Next(4000, 4500));
            Gauge5.Set(_random.Next(5000, 5500));
            Gauge6.Set(_random.Next(6000, 6500));
            Gauge7.Set(_random.Next(7000, 7500));
            Gauge8.Set(_random.Next(8000, 8500));
            Gauge9.Set(_random.Next(9000, 9500));

            _logger.LogInformation("{Source} has sent some data", nameof(DataService));            
        }
    }
}
