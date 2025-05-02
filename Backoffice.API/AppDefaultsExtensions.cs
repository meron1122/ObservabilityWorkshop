using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class AppDefaultsExtensions
{
    public static IHostApplicationBuilder AddAppDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAppOpenTelemetry();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureAppOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(static metrics =>
            {
                metrics.AddMeter("Todo.Api");
                metrics.AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        return builder;
    }
    
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRequestTimeouts(
            configure: static timeouts =>
                timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5)));

        builder.Services.AddOutputCache(
            configureOptions: static caching =>
                caching.AddPolicy("HealthChecks",
                    build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));

        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        var healthChecks = app.MapGroup("");

        healthChecks
            .CacheOutput("HealthChecks")
            .WithRequestTimeout("HealthChecks");

        // All health checks must pass for app to be
        // considered ready to accept traffic after starting
        healthChecks.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag
        // must pass for app to be considered alive
        healthChecks.MapHealthChecks("/alive", new()
        {
            Predicate = static r => r.Tags.Contains("live")
        });

        return app;
    }
}
