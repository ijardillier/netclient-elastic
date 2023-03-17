# Context

The purpose of this application is to show how to integrate:

- logs (via Serilog)
- health checks (via Microsoft AspNetCore HealthChecks)
- business metrics (via Prometheus) 
- traces (via Elastic APM or OpenTelemetry)
  
to an Elasticsearch cluster with .Net.

# Logs (via Serilog)

This application uses [Serilog](https://serilog.net/) to properly format logs.

## NuGet packages

### Serilog NuGet packages

Following Serilog NuGet packages are used to immplement logging: 

- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore): 
  
    Serilog logging for ASP.NET Core: this package routes ASP.NET Core log messages through Serilog, so you can get information about ASP.NET's internal operations written to the same Serilog sinks as your application events. With Serilog.AspNetCore installed and configured, you can write log messages directly through Serilog or any ILogger interface injected by ASP.NET. All loggers will use the same underlying implementation, levels, and destinations.

- [Serilog.Enrichers.Environment](https://github.com/serilog/serilog-enrichers-environment): 
  
    Enriches Serilog events with information from the process environment.

- [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration):

    A Serilog settings provider that reads from Microsoft.Extensions.Configuration sources, including .NET Core's appsettings.json file. By default, configuration is read from the Serilog section.

- [Serilog.Sinks.Console](https://github.com/serilog/serilog-sinks-console):

    A Serilog sink that writes log events to the Windows Console or an ANSI terminal via standard output. Coloring and custom themes are supported, including ANSI 256-color themes on macOS, Linux and Windows 10. The default output is plain text; JSON formatting can be plugged in using a package such as Serilog.Formatting.Compact. But for our needs, we will use the formatting provided by Elastic.

### Elastic NuGet packages

The following Elastic NuGet package is used to properly format logs for Elasticsearch:

- [Elastic.CommonSchema.Serilog](https://github.com/elastic/ecs-dotnet)

    Formats a Serilog event into a JSON representation that adheres to the Elastic Common Schema.

## Implementation

### NuGet packages

You have to add the following packages in your csproj file.

    <PackageReference Include="Elastic.CommonSchema.Serilog" Version="1.5.3" />
    <PackageReference Include="Serilog.AspNetCore" Version="6.1.0" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.2.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="3.4.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />

You can update the version to the latest available for your .Net version.

### Serilog provider

You then have to define Serilog as your log provider.

In your Program.cs file, add the ConfigureLogging and UseSerilog as described below: 

    public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((host, builder) => builder.ClearProviders().AddSerilog(host.Configuration))
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build();

The UseSerilog method sets Serilog as the logging provider. The AddSerilog method is a custom extension which will add Serilog to the logging pipeline and read the configuration from host configuration :

    public static ILoggingBuilder AddSerilog(this ILoggingBuilder builder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.AddSerilog();

        return builder;
    }

### Serilog HTTP Request logging

When using the default middleware for HTTP request logging, it will write HTTP request information like method, path, timing, status code and exception details in several events. To avoid this and use streamlined request loggin, you can use the middleware provided by Serilog.

Add this in Startup.cs before any handlers whose activities should be logged.

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseSerilogRequestLogging();

        app.UseEndpoints(endpoints =>
        {
            /// ...
        });

        // ...
    }

### Serilog configuration 

As the Serilog configuration is read from host configuration, we will now set all configuration we need to the appsettings file.

#### Production environment

In Production environment, we will prepare our logs for Elasticsearch ingestion, so use JSON format and add all needed information to ou logs. 

    {
        "Serilog": {
            "Using": [],
            "MinimumLevel": {
                "Default": "Warning",
                "Override": {
                    "Microsoft.Hosting": "Information",
                    "NetClient.Elastic": "Information"
                }
            },
            "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentUserName", "WithEnvironmentName", "WithProcessId", "WithThreadId"],
            "Properties": {
                "Domain": "NetClient",
                "DomainContext": "NetClient.Elastic"
            },
            "WriteTo": [
                { 
                    "Name": "Console",
                    "Args": {
                        "formatter": "Elastic.CommonSchema.Serilog.EcsTextFormatter, Elastic.CommonSchema.Serilog"
                    }
                }
            ]
        },
        // ...
    }

The configuration (for Production environment) will :

- set default log level to Warning except for "Microsoft.Hosting" "NetClient.Elastic" (our application) namespaces which will be Information
- enrich log with log context, machine name, and some other useful data when available
- add custom properties to each log event : Domain and DomainContext
- write logs to console, using the Elastic JSON formatter for Serilog

#### Development environment

In Development, generally, we won't want to display our logs in JSON format and we will prefer having log level to Debug and minimal information, so, we will override this in the appsettings.Development.json file.

    {
        "Serilog": {
            "MinimumLevel": {
                "Override": {
                    "NetClient.Elastic": "Debug"
                }
            },
            "WriteTo": [
                { 
                    "Name": "Console",
                    "Args": {
                        "outputTemplate": "-> [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    }
                }
            ]
        }
    }

## Sending logs to Elasticsearch

All the logs are written in the console, so they will be readable by using:

    docker container logs netcore-client

To send the logs to Elasticseach, you will have to configure a filebeat agent with docker autodiscover for example.

    filebeat.autodiscover:
        providers:
            - type: docker
            hints.enabled: true
            hints.default_config:
                type: container
                paths:
                - /var/lib/docker/containers/${data.container.id}/*.log

For more information about this filebeat configuration, you can have a look to : https://github.com/ijardillier/docker-elk/blob/master/filebeat/config/filebeat.yml

# Health checks (via Microsoft AspNetCore HealthChecks)

This application uses [Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0) to report the health of app infrastructure components.

## Healthchecks

ASP.NET Core offers Health Checks Middleware and libraries for reporting the health of app infrastructure components.

Health checks are exposed by an app as HTTP endpoints. Health check endpoints can be configured for various real-time monitoring scenarios:

- Health probes can be used by container orchestrators and load balancers to check an app's status. For example, a container orchestrator may respond to a failing health check by halting a rolling deployment or restarting a container. A load balancer might react to an unhealthy app by routing traffic away from the failing instance to a healthy instance.
- Use of memory, disk, and other physical server resources can be monitored for healthy status.
- Health checks can test an app's dependencies, such as databases and external service endpoints, to confirm availability and normal functioning.

Health checks are typically used with an external monitoring service or container orchestrator to check the status of an app. Before adding health checks to an app, decide on which monitoring system to use. The monitoring system dictates what types of health checks to create and how to configure their endpoints.

Source : [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0)

## NuGet packages

### Xabaril NuGet packages

The following Xabaril NuGet package is used:

- [AspNetCore.HealthChecks.UI.Client](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#configuration)

    Formats healthchecks endpoint response in a JSON representation.

## Implementation

### NuGet packages

You have to add the following packages in your csproj file.

    <PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="6.0.5" />

You can update the version to the latest available for your .Net version.

### HealthCheck service registration

The first step is to register the HealthCheck Service. This is done here in a custom extension which is used in the ConfigureServices of the Startup file.

    public virtual void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddCustomHealthCheck(Configuration)
        // ...     
    }

    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        IHealthChecksBuilder hcBuilder = services.AddHealthChecks();
        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
        return services;
    }

This "self" check is just here to say that if the endpoint responds, that's because the application is alive.

### HealthCheck endpoints maps

The second step is to map endpoints for health checks.

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        // ...

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });
    }

The first map exposes the /liveness endpoint with the self check described in the previous section.

The result of a call to http://localhost:8080/liveness will just be :

    Status code : 200 (Ok)
    Content : Healthy

The second map exposes the /hc endpoint with an aggregation of all healthchecks defined in a JSON format.

The result of a call to http://localhost:8080/hc will be :

    Status code : 200 (Ok)
    Content : {"status":"Healthy","totalDuration":"00:00:00.0027779","entries":{"self":{"data":{},"duration":"00:00:00.0008869","status":"Healthy","tags":[]}}}

## Sending healthchecks to Elasticsearch

All the healthchecks are available on the /hc endpoint.

To send the healthchecks to Elasticseach, you will have to configure a metricbeat agent with docker autodiscover for example.

    metricbeat.modules:
    - module: http
      metricsets:
      - json
      period: 10s
      hosts: ["localhost:8080"]
      namespace: "aspnet_healthchecks"
      path: "/hc"
  
For more information about this metricbeat configuration, you can have a look to : https://github.com/ijardillier/docker-elk/blob/master/metricbeat/config/metricbeat.yml
