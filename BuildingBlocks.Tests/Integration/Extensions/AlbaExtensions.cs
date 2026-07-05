using System.Text.Json;
using Alba;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Extensions;

/// <summary>
/// Extension methods for Alba scenario results to simplify test output logging.
/// </summary>
public static class AlbaExtensions
{
    /// <summary>
    /// Awaits the scenario result, prints the response body to the test output, and returns the same result for fluent chaining.
    /// JSON payloads are formatted for readability.
    /// </summary>
    public static async Task<IScenarioResult> PrintBody(
        this Task<IScenarioResult> resultTask,
        ITestOutputHelper output,
        string header = "Response Body:")
    {
        var result = await resultTask;
        var responseText = await result.ReadAsTextAsync();

        if (!string.IsNullOrWhiteSpace(responseText))
        {
            try
            {
                responseText = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<JsonElement>(responseText),
                    new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
            }
        }

        output.WriteLine(header);
        output.WriteLine(responseText);

        return result;
    }
}
