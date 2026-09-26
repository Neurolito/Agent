using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

internal class AnthropicPromptRequestHandler : SendMessageHandlerBase<AnthropicPromptRequest, AnthropicPromptResponse>
{
    private readonly IAnthropicPassthroughService _passthroughService;

    public AnthropicPromptRequestHandler(IAnthropicPassthroughService passthroughService)
    {
        _passthroughService = passthroughService;
    }

    public override async Task<AnthropicPromptResponse> Handle(AnthropicPromptRequest message)
    {
        return await _passthroughService.RunAsync(message, CancellationToken.None);
    }
}
