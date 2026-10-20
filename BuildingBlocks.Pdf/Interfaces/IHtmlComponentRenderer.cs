using Microsoft.AspNetCore.Components;

namespace BuildingBlocks.Pdf.Interfaces;

/// <summary>
/// Renders a Razor component to an HTML string, ready to be passed to <see cref="IPdfGenerator"/>.
/// Values written by the component are HTML-encoded.
/// </summary>
public interface IHtmlComponentRenderer
{
    /// <summary>
    /// Renders <typeparamref name="TComponent"/> with the given parameters, using Polish culture for dates and numbers.
    /// </summary>
    Task<string> RenderAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}
