namespace Tharga.Neurolito.Agent.Features.Ollama;

internal interface IEventService
{
    event EventHandler<EventArgs> CancelEvent;
    void OnCancel();
}