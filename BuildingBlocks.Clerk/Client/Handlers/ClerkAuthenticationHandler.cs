using System.Net.Http.Headers;
using BuildingBlocks.Clerk.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Clerk.Client.Handlers;

/// <summary>
/// Adds Clerk Backend API bearer authentication to every outgoing Clerk request.
/// </summary>
public sealed class ClerkAuthenticationHandler(IOptions<ClerkOptions> options) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var secretKey = options.Value.SecretKey;

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("Clerk SecretKey is not configured.");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

        return base.SendAsync(request, cancellationToken);
    }
}
