using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

namespace BuildingBlocks.Infrastructure.Serilog.Builders;

/// <summary>
/// Collects Serilog sink configuration actions for infrastructure-level logging setup.
/// </summary>
public sealed class SerilogBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly List<Action<LoggerConfiguration>> _configurationActions = [];

    internal SerilogBuilder(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    internal IEnumerable<Action<LoggerConfiguration>> GetConfigurationActions() => _configurationActions;

    /// <summary>
    /// Adds the console sink using text output in development and compact JSON otherwise.
    /// </summary>
    public SerilogBuilder AddConsole()
    {
        var isDevelopment = _environment.IsDevelopment();

        _configurationActions.Add(configuration =>
        {
            configuration.WriteTo.Async(writeTo =>
            {
                if (isDevelopment)
                {
                    writeTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "[{TraceId}] " +
                        "{SourceContext} " +
                        "{Message:lj}{NewLine}{Exception}");
                }
                else
                {
                    writeTo.Console(new CompactJsonFormatter());
                }
            });
        });

        return this;
    }

    /// <summary>
    /// Adds a daily rolling file sink.
    /// </summary>
    public SerilogBuilder AddFile()
    {
        _configurationActions.Add(configuration =>
        {
            configuration.WriteTo.Async(writeTo =>
            {
                writeTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "[{TraceId}] " +
                        "{SourceContext} " +
                        "{Message:lj}{NewLine}{Exception}");
            });
        });

        return this;
    }

    /// <summary>
    /// Adds the OpenTelemetry sink when an OTLP endpoint is configured.
    /// </summary>
    public SerilogBuilder AddOpenTelemetry()
    {
        var otlpEndpoint = _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var serviceName = _configuration["OTEL_SERVICE_NAME"] ?? _environment.ApplicationName;
        var environmentName = _environment.EnvironmentName;

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            _configurationActions.Add(configuration =>
            {
                configuration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;

                    options.IncludedData = IncludedData.TraceIdField
                        | IncludedData.SpanIdField
                        | IncludedData.SourceContextAttribute
                        | IncludedData.MessageTemplateTextAttribute
                        | IncludedData.MessageTemplateMD5HashAttribute;

                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["deployment.environment"] = environmentName
                    };
                });
            });
        }

        return this;
    }
}
