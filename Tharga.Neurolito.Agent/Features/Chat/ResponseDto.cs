using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Agent.Features.Chat;

public record ResponseDto
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }
}