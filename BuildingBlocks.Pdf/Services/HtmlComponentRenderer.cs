using System.Globalization;
using BuildingBlocks.Pdf.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Pdf.Services;

/// <summary>
/// <see cref="IHtmlComponentRenderer"/> on top of ASP.NET Core's <see cref="HtmlRenderer"/>.
/// Registered as scoped, so components can inject the scoped services of the current request.
/// </summary>
public sealed class HtmlComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : IHtmlComponentRenderer
{
    private static readonly CultureInfo Polish = new("pl-PL");

    /// <inheritdoc />
    public async Task<string> RenderAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var renderer = new HtmlRenderer(services, loggerFactory);

        // The renderer only works on its own dispatcher.
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            CultureInfo.CurrentCulture = Polish;
            CultureInfo.CurrentUICulture = Polish;

            var parameterView = parameters is null ? ParameterView.Empty : ParameterView.FromDictionary(parameters);
            var output = await renderer.RenderComponentAsync<TComponent>(parameterView);

            return output.ToHtmlString();
        });
    }
}
