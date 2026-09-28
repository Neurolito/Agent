using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

/// <summary>
/// Who cast a vote: a person, or a pipeline rating the answer on its own.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VoteSource
{
    /// <summary>A person judged the answer, for example by pressing thumbs up or down.</summary>
    Manual,

    /// <summary>An automated pipeline rated the answer without a person involved.</summary>
    Automatic
}
