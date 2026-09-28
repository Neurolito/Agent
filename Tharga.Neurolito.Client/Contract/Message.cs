namespace Tharga.Neurolito.Client.Contract;

public record Message
{
    public Role Role { get; init; }
    public string Content { get; init; }
}