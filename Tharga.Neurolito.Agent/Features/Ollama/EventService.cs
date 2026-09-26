namespace Tharga.Neurolito.Agent.Features.Ollama;

internal class EventService : IEventService
{
    public event EventHandler<EventArgs> CancelEvent;

    public void OnCancel()
    {
        CancelEvent?.Invoke(this, EventArgs.Empty);
    }
}