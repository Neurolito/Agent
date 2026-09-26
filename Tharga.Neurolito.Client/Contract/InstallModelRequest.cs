namespace Tharga.Neurolito.Client.Contract;

public record InstallModelRequest
{
    public required string Model { get; init; }
}