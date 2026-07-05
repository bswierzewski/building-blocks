namespace BuildingBlocks.Infrastructure.Identity.Options;

/// <summary>
/// Configures JWT bearer authentication for the API.
/// Any supplied validation input is applied; omitted values leave that validation path disabled.
/// </summary>
public sealed class IdentityOptions
{
    public const string SectionName = "Authentication:Identity";

    // @env: Authentication__Identity__Authority=
    public string? Authority { get; set; }

    // @env: Authentication__Identity__Issuer=
    public string? Issuer { get; set; }

    // @env: Authentication__Identity__Audience=
    public string? Audience { get; set; }

    // @env: Authentication__Identity__SigningKey=
    public string? SigningKey { get; set; }
}
