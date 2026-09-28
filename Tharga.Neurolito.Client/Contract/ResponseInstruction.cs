namespace Tharga.Neurolito.Client.Contract;

public record ResponseInstruction
{
    public ServiceBusResponse ServiceBusResponse { get; init; }
}