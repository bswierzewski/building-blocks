using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Verification state of an email address in Clerk.
/// </summary>
public sealed record ClerkEmailVerificationResponse(
    [property: JsonPropertyName("status")]
    string Status);
