namespace BuildingBlocks.Pdf.Models;

/// <summary>
/// A file the HTML refers to by name, e.g. <c>&lt;img src="logo.png"&gt;</c> for an asset named <c>logo.png</c>.
/// </summary>
public sealed record PdfAsset(string FileName, byte[] Content);
