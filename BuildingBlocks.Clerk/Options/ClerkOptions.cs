namespace BuildingBlocks.Clerk.Options;

/// <summary>
/// Configures access to the Clerk Backend API.
/// </summary>
public sealed class ClerkOptions
{
    public const string SectionName = "Clerk";

    // @env: Clerk__ApiBaseUrl=https://api.clerk.com
    public string ApiBaseUrl { get; set; } = "https://api.clerk.com";

    // @env: Clerk__SecretKey=
    public string? SecretKey { get; set; }
}
