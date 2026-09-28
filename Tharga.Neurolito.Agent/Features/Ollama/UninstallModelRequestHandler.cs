using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

public class UninstallModelRequestHandler : PostMessageHandlerBase<UninstallModelRequest>
{
    private readonly IOllamaService _ollamaService;

    public UninstallModelRequestHandler(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    public override async Task Handle(UninstallModelRequest message)
    {
        await _ollamaService.UninstallModelAsync(message.Model, true, CancellationToken.None);
    }
}