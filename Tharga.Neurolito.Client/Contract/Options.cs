using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

public record Options
{
    /// <summary>
    /// Controls randomness of the output. Lower values make responses more deterministic; higher values more creative.
    /// A value near 0 gives near-identical answers every time; a value near 2 produces wild, unpredictable output.
    /// Min: 0.0 | Max: 2.0 | Ollama default: 0.8
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    /// <summary>
    /// Nucleus sampling threshold. Only tokens whose cumulative probability exceeds this value are considered.
    /// Lower values make the model stick to the most likely words; higher values allow more variety in word choice.
    /// Min: 0.0 | Max: 1.0 | Ollama default: 0.9
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; init; }

    /// <summary>
    /// Limits the token pool to the top K most probable tokens at each step.
    /// A low value keeps responses focused and predictable; a high value allows more diverse and surprising word choices.
    /// Min: 1 | Max: 100 | Ollama default: 40
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; init; }

    /// <summary>
    /// Penalises repeated tokens to reduce repetition in output. 1.0 = no penalty.
    /// Increase this if the model keeps looping or restating the same phrases; decrease it to allow more natural repetition.
    /// Min: 0.0 | Max: 2.0 | Ollama default: 1.1
    /// </summary>
    [JsonPropertyName("repeat_penalty")]
    public float? RepeatPenalty { get; init; }

    /// <summary>
    /// Maximum number of tokens to generate in the response. -1 = unlimited.
    /// Lower values cut responses short, which is useful for brief answers or controlling costs; higher values allow longer, more complete replies.
    /// Min: -1 | Max: 32768 | Ollama default: -1
    /// </summary>
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; init; }

    /// <summary>
    /// Size of the context window (tokens). Larger values allow longer conversations but use more memory.
    /// Increase this if the model loses track of earlier parts of a long conversation; decrease it to reduce GPU memory usage.
    /// Min: 512 | Max: 131072 | Ollama default: 2048
    /// </summary>
    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; init; }

    /// <summary>
    /// One or more strings that will stop generation when encountered in the output.
    /// Useful for structured output — e.g. set to ["\n"] to get single-line responses, or ["###"] to stop at a section marker.
    /// Ollama default: null (no stop sequences)
    /// </summary>
    [JsonPropertyName("stop")]
    public string[] Stop { get; init; }
}
