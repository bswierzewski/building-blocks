using System.Text.Json.Serialization;

namespace BuildingBlocks.Clerk.Client.Models;

/// <summary>
/// Request body for updating Clerk user metadata.
/// </summary>
public sealed record UpdateClerkUserMetadataRequest(
    /// <summary>
    /// Public metadata patch applied to the Clerk user.
    /// </summary>
    [property: JsonPropertyName("public_metadata")]
    IReadOnlyDictionary<string, object?> PublicMetadata);
