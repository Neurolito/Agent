using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

/// <summary>
/// Asks an agent to run a one-token prompt on a model, to find out whether its engine can generate
/// at all. Sent by the server to a model it has rested after machine faults, instead of risking a
/// real job as the trial. Only sent to agents whose <see cref="Engine.SupportsSelfTest"/> is set.
/// </summary>
public record EngineSelfTestRequest
{
    public required string Model { get; init; }
}

/// <summary>What an agent found when it ran <see cref="EngineSelfTestRequest"/>.</summary>
public record EngineSelfTestResponse
{
    public required string Model { get; init; }

    /// <summary>Whether the engine produced an answer.</summary>
    public required bool Ok { get; init; }

    /// <summary>Why it did not, including the engine's own error text when it gave one.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Error { get; init; }

    public required double DurationMs { get; init; }
}
