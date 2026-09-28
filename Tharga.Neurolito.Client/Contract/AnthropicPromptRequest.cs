namespace Tharga.Neurolito.Client.Contract;

/// <summary>
/// An Anthropic request handed to an agent to run against its own engine.
/// </summary>
/// <remarks>
/// <see cref="RequestJson"/> is carried **opaquely**. The server does not parse it beyond resolving
/// the model, and the agent forwards it unread, which is what keeps tool use, thinking and vision
/// working without either of them knowing those features exist.
/// </remarks>
public record AnthropicPromptRequest
{
    public required Guid RequestId { get; init; }

    /// <summary>The engine-side model name, already resolved from the published id.</summary>
    public string Model { get; init; }

    /// <summary>The Anthropic request body, forwarded as it arrived.</summary>
    public required string RequestJson { get; init; }
}
