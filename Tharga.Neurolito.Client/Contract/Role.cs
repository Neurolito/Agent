using System.Text.Json.Serialization;

namespace Tharga.Neurolito.Client.Contract;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    System,
    User,
    Assistant,
    Tool
}