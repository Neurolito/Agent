namespace Tharga.Neurolito.Client.Contract;

public record PromptStatusResponse
{
    public Guid RequestId { get; init; }
    public PromptStatus Status { get; init; }
    public DateTime? CreatedAt { get; init; }
}
