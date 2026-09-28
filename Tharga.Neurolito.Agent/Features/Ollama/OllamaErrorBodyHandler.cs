using System.Net;
using System.Text.Json;

namespace Tharga.Neurolito.Agent.Features.Ollama;

/// <summary>
/// Puts Ollama's own error text into the exception a failed request raises.
/// </summary>
/// <remarks>
/// Ollama answers a failed generate with a status code and a JSON body saying why - for example that
/// a model needs more memory than is free. The HTTP client's exception keeps only the status line, so
/// the server would see "500 (Internal Server Error)" and nothing more. The status line is kept as it
/// was, because the server recognises a machine fault by it; the reason is appended.
/// </remarks>
internal class OllamaErrorBodyHandler : DelegatingHandler
{
    private const int MaxReasonLength = 500;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return response;

        //NOTE: Generation only. Pull, delete and show keep OllamaSharp's own handling of their codes.
        if (!IsGeneration(request.RequestUri)) return response;

        string reason;
        try
        {
            reason = Reason(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception)
        {
            reason = null;
        }

        if (string.IsNullOrWhiteSpace(reason)) return response;

        var status = (int)response.StatusCode;
        var phrase = response.ReasonPhrase ?? response.StatusCode.ToString();
        response.Dispose();
        throw new HttpRequestException($"Response status code does not indicate success: {status} ({phrase}). Ollama: {reason}", null, (HttpStatusCode)status);
    }

    internal static bool IsGeneration(Uri uri)
    {
        var path = uri?.AbsolutePath ?? string.Empty;
        return path.EndsWith("/api/chat", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/api/generate", StringComparison.OrdinalIgnoreCase);
    }

    internal static string Reason(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            {
                return Trim(error.GetString());
            }
        }
        catch (JsonException)
        {
        }

        return Trim(body);
    }

    private static string Trim(string value)
    {
        value = value?.Trim();
        if (string.IsNullOrEmpty(value)) return null;
        return value.Length <= MaxReasonLength ? value : value[..MaxReasonLength] + "…";
    }
}
