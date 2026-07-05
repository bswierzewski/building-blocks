using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Configuration;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that logs the incoming message type and its full payload before the handler executes.
/// Wolverine discovers <see cref="Before"/> by convention (method name).
/// </summary>
public static class LoggingMiddleware
{
    /// <summary>
    /// Executed before the message handler. Resolves the message from the envelope and writes an
    /// informational log entry containing the message type name and the full serialized payload.
    /// </summary>
    public static void Before(
        ILogger logger,
        IMessageContext messageContext)
    {
        var message = messageContext.Envelope?.Message;
        var messageType = message?.GetType().Name ?? "Unknown";

        // Log the message type and the full payload for traceability.
        logger.LogInformation("Handling: {MessageType}: {@Payload}", messageType, message);
    }
}

/// <summary>
/// Enables detailed payload logging for the decorated handler class or method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PayloadLoggingAttribute : ModifyChainAttribute
{
    /// <summary>
    /// Inserts <see cref="LoggingMiddleware.Before"/> at position 0 so it runs before any other middleware.
    /// </summary>
    public override void Modify(IChain chain, GenerationRules rules, IServiceContainer container)
    {
        chain.Middleware.Insert(0, new MethodCall(typeof(LoggingMiddleware), nameof(LoggingMiddleware.Before)));
    }
}
