namespace Tharga.Neurolito.Client.Contract;

public record ModelDetails
{
    public required string ParameterSize { get; init; }
    public long? ParameterCount { get; init; }
}