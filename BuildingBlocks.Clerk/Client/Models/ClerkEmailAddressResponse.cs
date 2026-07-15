using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Minimal Clerk email address projection.
/// </summary>
public sealed record ClerkEmailAddressResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("email_address")]
    string EmailAddress,
    [property: JsonPropertyName("verification")]
    ClerkEmailVerificationResponse? Verification);
