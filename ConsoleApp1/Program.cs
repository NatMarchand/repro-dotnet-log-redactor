using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

var hostBuilder = Host.CreateDefaultBuilder();
hostBuilder.ConfigureLogging(loggingBuilder =>
{
    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
    loggingBuilder.ClearProviders();
    loggingBuilder.AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.AddConsoleExporter();
        options.AddOtlpExporter();
    });
    loggingBuilder.EnableRedaction(o =>
    {
        o.ApplyDiscriminator = false;
    });
});

hostBuilder.ConfigureServices(services =>
{
    services.AddRedaction(o => o.SetFallbackRedactor<NullRedactor>());
    services.AddHostedService<ClockService>();
});

await hostBuilder.RunConsoleAsync();

public partial class ClockService(ILogger<ClockService> logger) : BackgroundService
{
    private readonly ILogger<ClockService> _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogTime(DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

    }

    [LoggerMessage(LogLevel.Information, "Its {time:T}")]
    partial void LogTime([UnknownDataClassification] DateTimeOffset time);
}