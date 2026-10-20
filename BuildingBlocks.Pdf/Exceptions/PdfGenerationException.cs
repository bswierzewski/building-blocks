namespace BuildingBlocks.Pdf.Exceptions;

/// <summary>
/// Thrown when the PDF service fails or cannot be reached. It is an infrastructure fault, not an error the user can fix.
/// </summary>
public sealed class PdfGenerationException(string message, Exception innerException) : Exception(message, innerException);
