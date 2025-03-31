# Serilog.Sinks.IBMLogs #

Serilog Sink that sends log events to IBM Cloud Logs <https://www.ibm.com/products/cloud-logs>

**Package** - [Serilog.Sinks.IBMLogs](http://nuget.org/packages/serilog.sinks.ibmlogs) | **Platforms** - netstandard2.0

## Getting started ##

First, create an IAM API key. To send logs, you need an API key linked to a Service ID, configured for the 'Cloud Logs' service with the 'Sender' role.

Enable the sink and log:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.IBMLogs(
        ingestUrl: "https://<INSTANCE_ID>.ingress.<REGION>.logs.cloud.ibm.com/logs/v1/singles",
        apiKey: "xxxxxxxxxxxxxxx-xxxx-xxxxxxxxxxxxxxxxxxxxxxx",
        applicationName: "cs-rest-test3",
        subsystemName: "cs-rest-test3")
    .CreateLogger();

var position = new { Latitude = 25, Longitude = 134 };
var elapsedMs = 34;
log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
```

Prints to console Livetail:

``` plaintext
[ 31/03/2025 01:48:20.617 ][INFO][cs-rest-test3][cs-rest-test3][desktop-r9hnrih]:{"Message":"Processed { Latitude: 25, Longitude: 134 } in 034 ms.","Elapsed":"34","Position":{"Latitude":25,"Longitude":134}}
```

## Log from ASP.NET Core & appsettings.json ##

Extra packages:

```shell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
```

Add `UseSerilog` to the Generic Host:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, logConfig) => logConfig.ReadFrom.Configuration(context.Configuration))
        .ConfigureWebHostDefaults(webBuilder => {
            webBuilder.UseStartup<Startup>();
        });
```

Add to `appsettings.json` configuration:

```json
{
    "Serilog": {
        "Using": [ "Serilog.Sinks.IBMLogs" ],
        "MinimumLevel": "Information",
        "WriteTo": [{
            "Name": "IBMLogs",
            "Args": {
                "ingestUrl": "https://<INSTANCE_ID>.ingress.<REGION>.logs.cloud.ibm.com/logs/v1/singles",
                "apiKey": "xxxxxxxxxxxxxxx-xxxx-xxxxxxxxxxxxxxxxxxxxxxx",
                "applicationName": "cs-rest-test3",
                "subsystemName": "cs-rest-test3"
            }
        }]
    }
}
```

[![Nuget](https://img.shields.io/nuget/v/serilog.sinks.ibmlogs.svg)](https://www.nuget.org/packages/Serilog.Sinks.IBMLogs/)
