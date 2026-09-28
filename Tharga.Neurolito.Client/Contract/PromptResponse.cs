using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

public record PromptResponse
{
    public required string Model { get; init; }
    public required string Output { get; init; }
    public required Dictionary<string, string> Tags { get; init; }
    public required Usage Usage { get; init; }

    [JsonIgnore]
    public ResponseInstruction ResponseInstruction { get; init; }
}