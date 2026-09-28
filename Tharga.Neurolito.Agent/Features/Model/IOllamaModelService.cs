using OllamaSharp.Models;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Model;

public interface IOllamaModelService
{
    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<LLModel> GetModelsAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<PullModelResponse> InstallModelAsync(string model, bool respondToServer, CancellationToken cancellationToken);
    Task UninstallModelAsync(string model, bool respondToServer, CancellationToken cancellationToken);
    Task<CapabilityResponse> BuildCapabilityAsync();
}