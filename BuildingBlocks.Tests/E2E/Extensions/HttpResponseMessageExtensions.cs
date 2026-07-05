using System.Text.Json;
using Xunit;

namespace BuildingBlocks.Tests.E2E.Extensions;

/// <summary>
/// Extension methods for HttpResponseMessage to simplify test output logging.
/// </summary>
public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Awaits the response task, prints the body to test output, and returns the same response for fluent chaining.
    /// </summary>
    public static async Task<HttpResponseMessage> PrintBody(
        this Task<HttpResponseMessage> responseTask,
        ITestOutputHelper output,
        string header = "Response Body:")
    {
        var response = await responseTask;
        await response.PrintBody(output, header);
        return response;
    }

    /// <summary>
    /// Prints the HTTP response body to the test output.
    /// JSON payloads are formatted for readability; other payloads are written as-is.
    /// </summary>
    public static async Task PrintBody(
        this HttpResponseMessage response,
        ITestOutputHelper output,
        string header = "Response Body:")
    {
        var responseText = await response.Content.ReadAsStringAsync();

        output.WriteLine(header);
        output.WriteLine(FormatResponseBody(responseText));
    }

    /// <summary>
    /// Formats JSON payloads for readable test output and leaves non-JSON payloads unchanged.
    /// </summary>
    private static string FormatResponseBody(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return "<empty>";

        try
        {
            using var jsonDocument = JsonDocument.Parse(responseText);
            return JsonSerializer.Serialize(
                jsonDocument.RootElement,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return responseText;
        }
    }
}
