namespace Tharga.Neurolito.Client.Contract;

public record ConsoleLogMessage
{
    public required Guid Instance { get; init; }
    public required ConsoleLogEntry[] Entries { get; init; }
}
