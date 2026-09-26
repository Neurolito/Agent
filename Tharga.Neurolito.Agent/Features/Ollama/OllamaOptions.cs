namespace Tharga.Neurolito.Agent.Features.Ollama;

public record OllamaOptions
{
    public string Address { get; set; }

    /// <summary>
    /// Models installed automatically at startup if not already present. Leave empty to install
    /// nothing and manage models by hand or from the web UI.
    /// </summary>
    public string[] Models { get; set; } = [];
}