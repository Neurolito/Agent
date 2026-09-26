using Tharga.Communication.MessageHandler;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Ollama;

internal class CancelPromptRequestHandler : PostMessageHandlerBase<CancelPromptRequest>
{
    private readonly IEventService _eventService;

    public CancelPromptRequestHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override Task Handle(CancelPromptRequest message)
    {
        _eventService.OnCancel();
        return Task.CompletedTask;
    }
}