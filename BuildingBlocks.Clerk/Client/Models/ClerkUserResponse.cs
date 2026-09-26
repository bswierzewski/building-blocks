using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Minimal Clerk user projection used to resolve a user before granting tenant access and to show tenant members.
/// </summary>
public sealed record ClerkUserResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("first_name")]
    string? FirstName,
    [property: JsonPropertyName("last_name")]
    string? LastName,
    [property: JsonPropertyName("public_metadata")]
    IReadOnlyDictionary<string, JsonElement>? PublicMetadata = null,
    [property: JsonPropertyName("email_addresses")]
    IReadOnlyList<ClerkEmailAddressResponse>? EmailAddresses = null,
    [property: JsonPropertyName("primary_email_address_id")]
    string? PrimaryEmailAddressId = null)
{
    /// <summary>
    /// Gets the user's primary email address, falling back to the first one, or <c>null</c> when there is none.
    /// </summary>
    public string? PrimaryEmail
        => (EmailAddresses?.FirstOrDefault(x => x.Id == PrimaryEmailAddressId) ?? EmailAddresses?.FirstOrDefault())
            ?.EmailAddress;

    /// <summary>
    /// Gets a string value from the user's public metadata, or <c>null</c> when it is missing or not a string.
    /// </summary>
    public string? GetPublicMetadataString(string key)
        => PublicMetadata is not null
            && PublicMetadata.TryGetValue(key, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
}
