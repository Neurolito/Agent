namespace Tharga.Neurolito.Client.Contract;

public record PromptRequest
{
    public required Guid RequestId { get; init; }
    public string Model { get; init; }
    public required Message[] Messages { get; init; }
    public Dictionary<string, string> Tags { get; init; }
    public ResponseInstruction ResponseInstruction { get; init; }
    public Options Options { get; init; }
    public int? TimeoutMinutes { get; init; }
}