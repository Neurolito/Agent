namespace Tharga.Neurolito.Client.Contract;

/// <summary>
/// A piece of an Anthropic response, sent by an agent as its engine produces it.
/// </summary>
/// <remarks>
/// <see cref="Chunk"/> is raw response text, not a parsed event. The engine already emits Anthropic's
/// own event sequence, so relaying the bytes is both simpler and more faithful than re-framing them
/// - and it means a future event type reaches the caller without a change here.
/// </remarks>
public record AnthropicChunkResponse
{
    public required Guid Instance { get; init; }
    public required Guid RequestId { get; init; }
    public required string Chunk { get; init; }
}
