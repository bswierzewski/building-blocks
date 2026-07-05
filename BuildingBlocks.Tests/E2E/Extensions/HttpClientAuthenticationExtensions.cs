using System.Net.Http.Headers;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Tests.E2E.Authentication;

namespace BuildingBlocks.Tests.E2E.Extensions;

/// <summary>
/// Helpers for expressing end-to-end HTTP requests in terms of <see cref="ICurrentUser"/>.
/// </summary>
public static class HttpClientAuthenticationExtensions
{
    public static AuthenticatedHttpClient As(this HttpClient httpClient, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(currentUser);

        return new AuthenticatedHttpClient(
            httpClient,
            currentUser.IsAuthenticated ? TestJwtTokenFactory.Create(currentUser) : null);
    }
}

/// <summary>
/// Thin request wrapper that applies a bearer token to each request without mutating shared client defaults.
/// </summary>
public sealed class AuthenticatedHttpClient(HttpClient httpClient, string? bearerToken)
{
    public Task<HttpResponseMessage> GetAsync(string? requestUri, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return SendAsync(request, cancellationToken);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return httpClient.SendAsync(request, cancellationToken);
    }
}
