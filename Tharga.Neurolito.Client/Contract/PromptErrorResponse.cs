namespace Tharga.Neurolito.Client.Contract;

public record PromptErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
}