using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Storage.Options;

/// <summary>
/// Configures access to an S3-compatible object store (AWS S3, Backblaze B2, ...).
/// Switching between a local emulator and production only changes these values.
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Endpoint of the S3-compatible service. Leave empty to talk to AWS S3 in <see cref="Region"/>.
    /// </summary>
    // @env: Storage__ServiceUrl=
    [Url]
    public string? ServiceUrl { get; set; }

    // @env: Storage__Region=us-east-1
    [Required]
    public string Region { get; set; } = "us-east-1";

    // @env: Storage__AccessKey=
    [Required]
    public string? AccessKey { get; set; }

    // @env: Storage__SecretKey=
    [Required]
    public string? SecretKey { get; set; }

    // @env: Storage__Bucket=
    [Required]
    public string? Bucket { get; set; }

    /// <summary>
    /// Addresses the bucket in the path (<c>host/bucket/key</c>) instead of the host name. Leave it empty to decide
    /// automatically: path style when <see cref="ServiceUrl"/> is set (emulators and most S3-compatible services need it),
    /// the host style of AWS S3 otherwise.
    /// </summary>
    // @env: Storage__ForcePathStyle=
    public bool? ForcePathStyle { get; set; }
}
