using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Minimal Clerk session projection used to revoke a user's active sessions.
/// </summary>
public sealed record ClerkSessionResponse(
    [property: JsonPropertyName("id")]
    string Id);
