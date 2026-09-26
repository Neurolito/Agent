namespace Tharga.Neurolito.Client.Contract;

public record VoteRequest
{
    public required Guid RequestId { get; init; }
    public required int Vote { get; init; }

    /// <summary>
    /// Whether a person or a pipeline cast the vote. Optional; a vote without it is stored with no source,
    /// so analytics can separate human judgement from pipeline signal only for votes that state it.
    /// </summary>
    public VoteSource? Source { get; init; }
}
