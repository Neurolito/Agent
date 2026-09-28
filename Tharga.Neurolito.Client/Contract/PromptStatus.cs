namespace Tharga.Neurolito.Client.Contract;

public enum PromptStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled,
    NotFound
}
