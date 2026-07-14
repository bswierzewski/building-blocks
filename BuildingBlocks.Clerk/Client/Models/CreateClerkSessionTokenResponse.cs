using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Response returned by Clerk after creating a session token.
/// </summary>
public sealed record CreateClerkSessionTokenResponse(
    [property: JsonPropertyName("jwt")]
    string Jwt);
