using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace JustSaying.Sample.Restaurant.KitchenConsole.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();

        // Add OTEL metrics
        services
            .AddOpenTelemetry()
            .WithMetrics((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                builder.AddOtlpExporter();
            })
            .WithTracing((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                    .AddAWSInstrumentation()
                    .AddHttpClientInstrumentation();

                if (environment.IsDevelopment())
                {
                    builder.SetSampler(new AlwaysOnSampler());
                }

                builder.AddOtlpExporter();
            });

        return services;
    }
}
