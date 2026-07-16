using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Minimal Clerk user projection used to resolve a user before granting organization access.
/// </summary>
public sealed record ClerkUserResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("first_name")]
    string? FirstName,
    [property: JsonPropertyName("last_name")]
    string? LastName);
