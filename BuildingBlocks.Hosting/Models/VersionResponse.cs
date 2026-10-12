namespace BuildingBlocks.Hosting.Models;

public sealed record VersionResponse(string? Version, string? Sha, DateTimeOffset? BuiltAt, string Environment);
