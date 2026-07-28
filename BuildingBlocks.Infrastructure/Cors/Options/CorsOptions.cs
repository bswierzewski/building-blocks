namespace BuildingBlocks.Infrastructure.Cors.Options;

/// <summary>
/// Configures browser origins allowed to call the API.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public const string FrontendPolicyName = "frontend";

    // @env: Cors__AllowedOrigins__0=https://localhost
    // @env: Cors__AllowedOrigins__1=https://app.dev.localhost
    public string[] AllowedOrigins { get; set; } = [];
}
