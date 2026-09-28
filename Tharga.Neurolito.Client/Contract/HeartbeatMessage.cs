namespace Tharga.Neurolito.Client.Contract;

public record HeartbeatMessage
{
    public required int Counter { get; init; }
}