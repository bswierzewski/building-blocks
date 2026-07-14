using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Request body for creating a Clerk session.
/// </summary>
public sealed record CreateClerkSessionRequest(
    [property: JsonPropertyName("user_id")]
    string UserId);
