namespace BuildingBlocks.Pdf.Models;

/// <summary>
/// Page margins in millimetres.
/// </summary>
public sealed record PdfMargins(double Top, double Right, double Bottom, double Left)
{
    public static PdfMargins Default { get; } = All(10);

    public static PdfMargins All(double millimetres) => new(millimetres, millimetres, millimetres, millimetres);
}
