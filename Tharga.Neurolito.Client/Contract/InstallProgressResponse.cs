namespace Tharga.Neurolito.Client.Contract;

public record InstallProgressResponse
{
    public required Guid Instance { get; init; }
    public double Percent { get; init; }
}