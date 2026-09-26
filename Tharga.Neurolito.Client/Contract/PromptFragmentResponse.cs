namespace Tharga.Neurolito.Client.Contract;

public record PromptFragmentResponse
{
    public required Guid Instance { get; init; }
    public required Guid RequestId { get; init; }
    public required string Output { get; init; }
    public required int Tokens { get; init; }
    public required decimal? WorkUnits { get; init; }
    public required decimal? Throughput { get; init; }
    public required decimal TotalDurationMs { get; init; }
}