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
    [Get("/v1/users?email_address[]={email}")]
    Task<IReadOnlyList<ClerkUserResponse>> GetUsersByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Clerk users by their identifiers. Clerk accepts up to 100 identifiers and returns 10 users unless a
    /// larger <paramref name="limit"/> is given (max 500), so callers should pass at most 100 ids and a matching limit.
    /// </summary>
    [Get("/v1/users")]
    Task<IReadOnlyList<ClerkUserResponse>> GetUsersByIdsAsync(
        [Query(CollectionFormat.Multi)]
        [AliasAs("user_id[]")] IEnumerable<string> userIds,
        [AliasAs("limit")] int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates public metadata for a Clerk user.
    /// </summary>
    [Patch("/v1/users/{userId}/metadata")]
    Task UpdateUserMetadataAsync(
        string userId,
        [Body] UpdateClerkUserMetadataRequest request,
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
