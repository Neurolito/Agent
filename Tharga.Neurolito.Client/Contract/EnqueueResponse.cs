namespace Tharga.Neurolito.Client.Contract;

public record EnqueueResponse
{
    public required bool Success { get; init; }
    public Guid? RequestId { get; init; }
    public Uri InfoLinq { get; init; }
    public string Message { get; init; }
}