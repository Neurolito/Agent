using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

public record AgentPromptResponse
{
    public required string Model { get; init; }
    public required string Output { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, string> Tags { get; init; }

    public required Usage Usage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ResponseInstruction ResponseInstruction { get; init; }
}