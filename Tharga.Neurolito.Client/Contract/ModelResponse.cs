namespace Tharga.Neurolito.Client.Contract;

public record ModelResponse
{
    public required LLModel[] Models { get; init; }
}