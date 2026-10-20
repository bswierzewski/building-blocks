using BuildingBlocks.Pdf.Exceptions;
using BuildingBlocks.Pdf.Interfaces;
using BuildingBlocks.Pdf.Models;
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.Pages;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Pages;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace BuildingBlocks.Pdf.Services;

/// <summary>
/// <see cref="IPdfGenerator"/> that converts HTML with a Gotenberg service (headless Chromium).
/// </summary>
public sealed class GotenbergPdfGenerator(GotenbergSharpClient client) : IPdfGenerator
{
    /// <inheritdoc />
    public async Task<byte[]> GenerateAsync(PdfSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(source.Html))
            throw new ArgumentException("The HTML to convert is empty.", nameof(source));

        var request = new HtmlRequestBuilder()
            .AddDocument(document =>
            {
                document.SetBody(source.Html);

                if (source.HeaderHtml is not null)
                    document.SetHeader(source.HeaderHtml);

                if (source.FooterHtml is not null)
                    document.SetFooter(source.FooterHtml);
            })
            .WithPageProperties(page => Apply(page, source.Page));

        if (source.Assets.Count > 0)
            request.WithAssets(assets =>
            {
                foreach (var asset in source.Assets)
                    assets.AddItem(asset.FileName, asset.Content);
            });

        try
        {
            await using var pdf = await client.HtmlToPdfAsync(request, cancellationToken);
            using var buffer = new MemoryStream();
            await pdf.CopyToAsync(buffer, cancellationToken);

            return buffer.ToArray();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                                   && ex is GotenbergApiException or HttpRequestException or TaskCanceledException)
        {
            // The HTML is not part of the message: it can hold personal data.
            throw new PdfGenerationException("The PDF service failed to generate the document.", ex);
        }
    }

    private static void Apply(PagePropertyBuilder page, PdfPageOptions options)
        => page.SetPaperSize(ToPaperSizes(options.Size))
            .SetLandscape(options.Landscape)
            .SetPrintBackground(options.PrintBackground)
            .SetMargins(
                Dimension.FromMillimeters(options.Margins.Top),
                Dimension.FromMillimeters(options.Margins.Right),
                Dimension.FromMillimeters(options.Margins.Bottom),
                Dimension.FromMillimeters(options.Margins.Left));

    private static PaperSizes ToPaperSizes(PdfPaperSize size) => size switch
    {
        PdfPaperSize.A3 => PaperSizes.A3,
        PdfPaperSize.A4 => PaperSizes.A4,
        PdfPaperSize.A5 => PaperSizes.A5,
        PdfPaperSize.Letter => PaperSizes.Letter,
        PdfPaperSize.Legal => PaperSizes.Legal,
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Unknown paper size.")
    };
}
