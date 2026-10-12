using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Json.Extensions;

/// <summary>
/// One place for how the API writes and reads JSON.
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Configures the JSON options shared by the API, Wolverine HTTP and the OpenAPI document. Enums are sent and
    /// accepted as their names, so the API contract and the clients generated from it show the real values instead of
    /// opaque integers. Add further serializer settings here so every host gets them.
    /// </summary>
    public static IServiceCollection AddJsonSerialization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.Configure<JsonOptions>(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
}
