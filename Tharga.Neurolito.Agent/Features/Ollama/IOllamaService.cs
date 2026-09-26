using OllamaSharp.Models;
using Tharga.Neurolito.Agent.Features.Chat;
using Tharga.Neurolito.Agent.Features.Model;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

public interface IOllamaService
{
    [Obsolete($"Use {nameof(IOllamaModelService)} instead.")]
    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);

    [Obsolete($"Use {nameof(IOllamaModelService)} instead.")]
    IAsyncEnumerable<LLModel> GetModelsAsync(CancellationToken cancellationToken = default);

    [Obsolete($"Use {nameof(IOllamaModelService)} instead.")]
    IAsyncEnumerable<PullModelResponse> InstallModelAsync(string model, bool respondToServer, CancellationToken cancellationToken);

    [Obsolete($"Use {nameof(IOllamaModelService)} instead.")]
    Task UninstallModelAsync(string model, bool respondToServer, CancellationToken cancellationToken);

    Task<AgentPromptResponse> GetResponseAsync(RequestDto request, bool respondToServer, CancellationToken cancellationToken);

    [Obsolete($"Use {nameof(IOllamaModelService)} instead.")]
    Task<CapabilityResponse> BuildCapabilityAsync();
}