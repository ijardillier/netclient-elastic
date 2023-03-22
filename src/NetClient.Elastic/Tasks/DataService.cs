using Microsoft.Extensions.Options;
using Prometheus;
namespace NetClient.Elastic.Tasks
{
    public class DataService : BackgroundService
    {
        private readonly ILogger<DataService> _logger;
        private readonly Settings _settings;
        private readonly Random _random = new Random();

        private static readonly GaugeConfiguration configuration = new GaugeConfiguration { LabelNames = new[] { "service" }};
        private readonly Gauge Gauge1 = Metrics.CreateGauge("myapp_gauge1", "A simple gauge 1", configuration);
        private readonly Gauge Gauge2 = Metrics.CreateGauge("myapp_gauge2", "A simple gauge 2", configuration);
        private readonly Gauge Gauge3 = Metrics.CreateGauge("myapp_gauge3", "A simple gauge 3", configuration);
        private readonly Gauge Gauge4 = Metrics.CreateGauge("myapp_gauge4", "A simple gauge 4", configuration);
        private readonly Gauge Gauge5 = Metrics.CreateGauge("myapp_gauge5", "A simple gauge 5", configuration);
        private readonly Gauge Gauge6 = Metrics.CreateGauge("myapp_gauge6", "A simple gauge 6", configuration);
        private readonly Gauge Gauge7 = Metrics.CreateGauge("myapp_gauge7", "A simple gauge 7", configuration);
        private readonly Gauge Gauge8 = Metrics.CreateGauge("myapp_gauge8", "A simple gauge 8", configuration);
        private readonly Gauge Gauge9 = Metrics.CreateGauge("myapp_gauge9", "A simple gauge 9", configuration);
  
        public DataService(IOptions<Settings> settings, ILogger<DataService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("{Source} background task is starting.", nameof(DataService));

            cancellationToken.Register(() => _logger.LogDebug("#1 {Source} background task is stopping.", nameof(DataService)));

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Source} background task is doing background work.", nameof(DataService));

                SendData();

                await Task.Delay(_settings.DataServiceExecutionDelay, cancellationToken);
            }

            _logger.LogDebug("{Source} background task is stopping.", nameof(DataService));

            await Task.CompletedTask;
        }

        private void SendData()
        {
            _logger.LogDebug("{Source} is sending data.", nameof(DataService));

            Gauge1.WithLabels("service1").Set(_random.Next(1000, 2000));
            Gauge2.WithLabels("service1").Set(_random.Next(2000, 3000));
            Gauge3.WithLabels("service1").Set(_random.Next(3000, 4000));
            Gauge4.WithLabels("service2").Set(_random.Next(4000, 5000));
            Gauge5.WithLabels("service2").Set(_random.Next(5000, 6000));
            Gauge6.WithLabels("service2").Set(_random.Next(6000, 7000));
            Gauge7.WithLabels("service3").Set(_random.Next(7000, 8000));
            Gauge8.WithLabels("service3").Set(_random.Next(8000, 9000));
            Gauge9.WithLabels("service3").Set(_random.Next(9000, 10000));

            _logger.LogInformation("{Source} has sent some data", nameof(DataService));            
        }
    }
}
