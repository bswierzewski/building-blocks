using BuildingBlocks.Clerk.Client.Models;
using Refit;

namespace BuildingBlocks.Clerk.Client;

/// <summary>
/// Defines the Clerk Backend API endpoints used by applications.
/// </summary>
public interface IClerkHttpClient
{
    /// <summary>
    /// Creates a new Clerk session for the supplied user.
    /// </summary>
    [Post("/v1/sessions")]
    Task<CreateClerkSessionResponse> CreateSessionAsync(
        [Body] CreateClerkSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a JWT for an existing Clerk session.
    /// </summary>
    [Post("/v1/sessions/{sessionId}/tokens")]
    Task<CreateClerkSessionTokenResponse> CreateSessionTokenAsync(
        string sessionId,
        [Body] CreateClerkSessionTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates public metadata for a Clerk user.
    /// </summary>
    [Patch("/v1/users/{userId}/metadata")]
    Task UpdateUserMetadataAsync(
        string userId,
        [Body] UpdateClerkUserMetadataRequest request,
        CancellationToken cancellationToken = default);
}
