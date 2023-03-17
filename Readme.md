# Context

The purpose of this application is to show how to integrate:

- logs (via Serilog)
- metrics (via Prometheus) 
- Elastic APM (traces)
  
  to an Elasticsearch cluster with .Net.

# Logs

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

The following Elastic NuGet ackage is used to properly format logs for Elasticsearch:

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

The configuration (for Production environment) will :

- set default log level to Warning except for "Microsoft.Hosting" "NetClient.Elastic" (our application) namespaces which will be Information
- enrich log with log context, machine name, and some other useful data when available
- add custom properties to each log event : Domain and DomainContext
- write logs to console, using the Elastic JSON formatter for Serilog

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

#### Development environment

In Development, generally, we won't want to display our logs in JSON format, with minimal information and we will prefer having log level to Debug, so, we will override this in the appsettings.Development.json file.

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
