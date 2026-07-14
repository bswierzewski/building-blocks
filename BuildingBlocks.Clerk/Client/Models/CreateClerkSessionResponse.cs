using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Response returned by Clerk after creating a session.
/// </summary>
public sealed record CreateClerkSessionResponse(
    [property: JsonPropertyName("id")]
    string Id);
