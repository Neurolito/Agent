using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

public class InstallModelRequestHandler : PostMessageHandlerBase<InstallModelRequest>
{
    private readonly IOllamaService _ollamaService;

    public InstallModelRequestHandler(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    public override async Task Handle(InstallModelRequest message)
    {
        await _ollamaService.InstallModelAsync(message.Model, true, CancellationToken.None).ToArrayAsync();
    }
}