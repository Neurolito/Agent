using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

/// <summary>How far outside its own team a job is allowed to look for an agent.</summary>
/// <remarks>
/// Orthogonal to agent ownership. Ownership decides who provides the machine; this decides whether
/// a job is willing to run somewhere it does not own. Both have to say yes: a job set to
/// <see cref="Any"/> still only reaches outside agents that opted in.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentReach
{
    /// <summary>
    /// The job may run on its own team's agents, and on other teams' agents that accept outside work.
    /// </summary>
    Any,

    /// <summary>
    /// The job runs only on its own team's agents, whatever anyone else has volunteered.
    /// </summary>
    /// <remarks>
    /// For work a team will not put on hardware it does not control, whatever capacity is going
    /// spare. A job set this way waits for its own fleet rather than borrowing.
    /// </remarks>
    TeamOnly
}
