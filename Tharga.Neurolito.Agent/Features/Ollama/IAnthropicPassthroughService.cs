using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

/// <summary>
/// Runs an Anthropic request against this agent's engine, streaming the answer back to the server.
/// </summary>
internal interface IAnthropicPassthroughService
{
    Task<AnthropicPromptResponse> RunAsync(AnthropicPromptRequest request, CancellationToken cancellationToken);
}
