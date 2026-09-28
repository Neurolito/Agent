using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Agent.Features.Chat;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

internal class PromptRequestHandler : SendMessageHandlerBase<PromptRequest, AgentPromptResponse>
{
    private readonly IOllamaService _ollamaService;

    public PromptRequestHandler(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    public override async Task<AgentPromptResponse> Handle(PromptRequest message)
    {
        var request = new RequestDto
        {
            RequestId = message.RequestId,
            Model = message.Model,
            Messages = message.Messages,
            Tags = message.Tags,
            ResponseInstruction = message.ResponseInstruction,
            Options = message.Options,
            TimeoutMinutes = message.TimeoutMinutes
        };

        return await _ollamaService.GetResponseAsync(request, true, CancellationToken.None);
    }
}