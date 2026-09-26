namespace Tharga.Neurolito.Client.Contract;

[Obsolete($"Use {nameof(QueueRequestV2)} instead.")]
public record QueueRequest
{
    /// <summary>
    /// Input.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Model name.
    /// </summary>
    public string Model { get; init; }

    public Dictionary<string, string> Tags { get; init; }

    public ResponseInstruction ResponseInstruction { get; init; }
}