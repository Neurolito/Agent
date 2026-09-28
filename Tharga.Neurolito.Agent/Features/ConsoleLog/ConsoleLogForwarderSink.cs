using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;
using Tharga.Communication.Client;
using Tharga.Communication.Client.Communication;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.ConsoleLog;

internal sealed class ConsoleLogForwarderSink : ILogEventSink, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentQueue<ConsoleLogEntry> _buffer = new();
    private readonly Timer _timer;
    private const int MaxBufferSize = 500;
    private const int FlushIntervalMs = 1000;

    private IClientCommunication _clientCommunication;
    private Guid? _instance;

    public ConsoleLogForwarderSink(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _timer = new Timer(FlushCallback, null, FlushIntervalMs, FlushIntervalMs);
    }

    public void Emit(LogEvent logEvent)
    {
        //NOTE: A connection drop is summarised before it is sent: it happens on every server deploy, and a
        //full stack trace in the console read as a failure. The server does the same for older agents.
        var display = ConsoleLogDisplay.Of(logEvent.RenderMessage(), logEvent.Exception?.ToString());
        var entry = new ConsoleLogEntry
        {
            Timestamp = logEvent.Timestamp.UtcDateTime,
            Level = display.IsConnectionDrop ? LogEventLevel.Information.ToString() : logEvent.Level.ToString(),
            Message = display.Message,
            Exception = display.IsConnectionDrop ? null : display.Detail
        };

        _buffer.Enqueue(entry);

        while (_buffer.Count > MaxBufferSize)
        {
            _buffer.TryDequeue(out _);
        }
    }

    private void FlushCallback(object state)
    {
        _ = FlushAsync();
    }

    private async Task FlushAsync()
    {
        if (_buffer.IsEmpty) return;

        try
        {
            _clientCommunication ??= _serviceProvider.GetService<IClientCommunication>();
            _instance ??= _serviceProvider.GetService<IInstanceService>()?.AgentInstanceKey;
        }
        catch
        {
            return;
        }

        if (_clientCommunication == null || _instance == null || !_clientCommunication.IsConnected) return;

        var entries = new List<ConsoleLogEntry>();
        while (_buffer.TryDequeue(out var entry))
        {
            entries.Add(entry);
        }

        if (entries.Count == 0) return;

        try
        {
            await _clientCommunication.PostAsync(new ConsoleLogMessage
            {
                Instance = _instance.Value,
                Entries = entries.ToArray()
            });
        }
        catch
        {
            foreach (var entry in entries)
            {
                _buffer.Enqueue(entry);
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
