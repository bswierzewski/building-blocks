using BuildingBlocks.Clerk.Client.Models;
using Refit;

namespace BuildingBlocks.Clerk.Client;

/// <summary>
/// Defines the Clerk Backend API endpoints used by applications.
/// </summary>
public interface IClerkHttpClient
{
    /// <summary>
    /// Finds Clerk users with the supplied email address.
    /// </summary>
    [Get("/v1/users?email_address={emailAddress}")]
    Task<IReadOnlyList<ClerkUserResponse>> GetUsersByEmailAsync(
        string emailAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds Clerk users with the supplied user ID.
    /// </summary>
    [Get("/v1/users?user_id={userId}")]
    Task<IReadOnlyList<ClerkUserResponse>> GetUsersByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates public metadata for a Clerk user.
    /// </summary>
    [Patch("/v1/users/{userId}/metadata")]
    Task UpdateUserMetadataAsync(
        string userId,
        [Body] UpdateClerkUserMetadataRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active sessions for the supplied user.
    /// </summary>
    [Get("/v1/sessions?status=active")]
    Task<IReadOnlyList<ClerkSessionResponse>> GetActiveSessionsAsync(
        [AliasAs("user_id")] string userId,
        [AliasAs("limit")] int limit,
        [AliasAs("offset")] int offset,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a Clerk session and signs its user out from the associated client.
    /// </summary>
    [Post("/v1/sessions/{sessionId}/revoke")]
    Task RevokeSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    // Testing endpoints, is only used by integration tests.

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
}
