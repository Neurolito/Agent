namespace Tharga.Neurolito.Client.Contract;

public record ConsoleLogEntry
{
    public required DateTime Timestamp { get; init; }
    public required string Level { get; init; }
    public required string Message { get; init; }
    public string Exception { get; init; }
}
