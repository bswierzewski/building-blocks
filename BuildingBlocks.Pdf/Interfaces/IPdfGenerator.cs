using BuildingBlocks.Pdf.Models;

namespace BuildingBlocks.Pdf.Interfaces;

/// <summary>
/// Turns a ready HTML document into a PDF. The HTML itself comes from the caller, for example from
/// <see cref="IHtmlComponentRenderer"/>.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates the PDF and returns its bytes.
    /// </summary>
    /// <exception cref="Exceptions.PdfGenerationException">The PDF service failed or could not be reached.</exception>
    Task<byte[]> GenerateAsync(PdfSource source, CancellationToken cancellationToken = default);
}
