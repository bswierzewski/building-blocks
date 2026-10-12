namespace BuildingBlocks.Hosting.Options;

/// <summary>
/// Describes the build of the running application. Set by the Docker build; empty for local runs.
/// </summary>
public sealed class BuildOptions
{
    public const string SectionName = "Build";

    // @env: Build__Version=
    public string? Version { get; set; }

    // @env: Build__Sha=
    public string? Sha { get; set; }

    // @env: Build__BuiltAt=
    public DateTimeOffset? BuiltAt { get; set; }
}
