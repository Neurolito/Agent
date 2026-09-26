using Tharga.Communication.Client.Communication;
using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

internal class CapabilityRequestHandler : PostMessageHandlerBase<CapabilityRequest>
{
    private readonly IOllamaService _ollamaService;
    private readonly IClientCommunication _clientCommunication;

    public CapabilityRequestHandler(IOllamaService ollamaService, IClientCommunication clientCommunication)
    {
        _ollamaService = ollamaService;
        _clientCommunication = clientCommunication;
    }

    public override async Task Handle(CapabilityRequest message)
    {
        var capability = await _ollamaService.BuildCapabilityAsync();
        await _clientCommunication.PostAsync(capability);
    }
}