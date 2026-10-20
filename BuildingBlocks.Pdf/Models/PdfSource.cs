namespace BuildingBlocks.Pdf.Models;

/// <summary>
/// Everything needed to produce one PDF.
/// </summary>
/// <param name="Html">A complete HTML document. Reach files only through <see cref="Assets"/>, never by outside address.</param>
public sealed record PdfSource(string Html)
{
    /// <summary>
    /// Files the HTML refers to by name (images, fonts, styles).
    /// </summary>
    public IReadOnlyList<PdfAsset> Assets { get; init; } = [];

    /// <summary>
    /// Optional page header, a complete HTML document as the PDF service expects it.
    /// </summary>
    public string? HeaderHtml { get; init; }

    /// <summary>
    /// Optional page footer, a complete HTML document as the PDF service expects it.
    /// </summary>
    public string? FooterHtml { get; init; }

    public PdfPageOptions Page { get; init; } = new();
}
