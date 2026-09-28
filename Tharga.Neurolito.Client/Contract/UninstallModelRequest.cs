namespace Tharga.Neurolito.Client.Contract;

public record UninstallModelRequest
{
    public required string Model { get; init; }
}