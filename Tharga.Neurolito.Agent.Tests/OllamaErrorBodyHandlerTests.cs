using System.Net;
using Tharga.Neurolito.Agent.Features.Ollama;

namespace Tharga.Neurolito.Agent.Tests;

/// <summary>
/// Ollama's own reason for a failed generation reaches the server, instead of only "500 (Internal Server Error)".
/// </summary>
public class OllamaErrorBodyHandlerTests
{
    private const string StatusLine = "Response status code does not indicate success: 500 (Internal Server Error).";

    [Fact]
    public async Task AFailedGenerateCarriesOllamasReason()
    {
        var client = Client(HttpStatusCode.InternalServerError, "{\"error\":\"model requires more system memory (41.2 GiB) than is available (26.7 GiB)\"}");

        var e = await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync("http://localhost:11434/api/chat", new StringContent("{}")));

        Assert.Contains("model requires more system memory", e.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, e.StatusCode);
    }

    [Fact]
    public async Task TheStatusLineIsKeptWordForWord()
    {
        //The server recognises a machine fault by this line, so the reason is only appended.
        var client = Client(HttpStatusCode.InternalServerError, "{\"error\":\"llama runner process has terminated\"}");

        var e = await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync("http://localhost:11434/api/generate", new StringContent("{}")));

        Assert.StartsWith(StatusLine, e.Message);
    }

    [Fact]
    public async Task OtherEndpointsKeepTheirOwnHandling()
    {
        var client = Client(HttpStatusCode.NotFound, "{\"error\":\"model not found\"}");

        var response = await client.PostAsync("http://localhost:11434/api/show", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ASuccessPassesThrough()
    {
        var client = Client(HttpStatusCode.OK, "{}");

        var response = await client.PostAsync("http://localhost:11434/api/chat", new StringContent("{}"));

        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("{\"error\":\"out of memory\"}", "out of memory")]
    [InlineData("plain text failure", "plain text failure")]
    [InlineData("", null)]
    public void TheReasonIsReadFromTheBody(string body, string? expected)
    {
        Assert.Equal(expected, OllamaErrorBodyHandler.Reason(body));
    }

    private static HttpClient Client(HttpStatusCode status, string body)
    {
        return new HttpClient(new OllamaErrorBodyHandler { InnerHandler = new Fixed(status, body) });
    }

    private sealed class Fixed(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}
