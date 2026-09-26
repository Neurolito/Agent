namespace Tharga.Neurolito.Client.Contract;

/// <summary>
/// How an agent's Anthropic request ended, once every chunk has been sent.
/// </summary>
/// <remarks>
/// The answer itself arrives as <see cref="AnthropicChunkResponse"/> messages. This carries only the
/// outcome, so a failure that happened before any chunk can still be reported as a status code
/// rather than as silence.
/// </remarks>
public record AnthropicPromptResponse
{
    public required int StatusCode { get; init; }

    /// <summary>The engine's own error body when it failed, or null.</summary>
    /// <remarks>
    /// Kept verbatim: a client's retry logic matches on the engine's wording, so wrapping it in an
    /// envelope of ours breaks recovery even when the status code survives.
    /// </remarks>
    public string ErrorJson { get; init; }

    /// <summary>Whether any chunk was sent before this outcome.</summary>
    public bool HasStreamed { get; init; }
}
