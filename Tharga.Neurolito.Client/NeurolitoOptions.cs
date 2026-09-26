namespace Tharga.Neurolito.Client;

public record NeurolitoOptions
{
    public string ServerAddress { get; set; } = "https://neurolito.com/api/";
    public string ApiKey { get; set; }
}