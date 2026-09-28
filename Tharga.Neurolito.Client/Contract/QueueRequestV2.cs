namespace Tharga.Neurolito.Client.Contract;

public record QueueRequestV2
{
    public string Model { get; init; }
    public required Message[] Messages { get; init; }
    public Options Options { get; init; }
    public Dictionary<string, string> Tags { get; init; }
    public ResponseInstruction ResponseInstruction { get; init; }
    public int? MaxProcessTimeSeconds { get; init; }

    /// <summary>
    /// Whether this job may run on another team's agent. Null takes the team's default.
    /// </summary>
    /// <remarks>
    /// Nullable so "did not say" is distinct from "said Any". A caller that omits it gets its team's
    /// setting; one that sends <see cref="AgentReach.TeamOnly"/> overrides a team default of
    /// <see cref="AgentReach.Any"/> for this job alone, which is the point of having it per job.
    /// </remarks>
    public AgentReach? Reach { get; init; }
}