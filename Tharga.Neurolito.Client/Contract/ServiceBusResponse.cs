namespace Tharga.Neurolito.Client.Contract;

public record ServiceBusResponse
{
    public string ConnectionString { get; init; }
    public string QueueName { get; init; }
}