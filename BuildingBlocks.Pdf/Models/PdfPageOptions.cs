namespace BuildingBlocks.Pdf.Models;

/// <summary>
/// How the page looks: A4 portrait with 10 mm margins and backgrounds printed, unless changed.
/// </summary>
public sealed record PdfPageOptions
{
    public PdfPaperSize Size { get; init; } = PdfPaperSize.A4;

    public bool Landscape { get; init; }

    public bool PrintBackground { get; init; } = true;

    public PdfMargins Margins { get; init; } = PdfMargins.Default;
}
