namespace Tharga.Neurolito.Client.Contract;

public record InputDto
{
    public required string Prompt { get; init; }
    public required string Model { get; init; }
}