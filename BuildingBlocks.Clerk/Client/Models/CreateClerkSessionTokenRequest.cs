using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Request body for creating a Clerk session token.
/// </summary>
public sealed record CreateClerkSessionTokenRequest(
    [property: JsonPropertyName("expires_in_seconds")]
    int ExpiresInSeconds);
