namespace Tharga.Neurolito.Client.Contract;

public record CanConnectResponse
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public required Uri Address { get; init; }
}