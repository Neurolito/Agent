using Microsoft.AspNetCore.SignalR.Client;
using Quilt4Net.Toolkit;
using Quilt4Net.Toolkit.Features.Health;
using Tharga.Communication.Client;
using Tharga.Neurolito.Agent.Features.Ollama;

namespace Tharga.Neurolito.Agent.Framework;

internal class ComponentService : IComponentService
{
    private readonly IOllamaService _ollamaService;
    private readonly ISignalRHostedService _signalRConnectionState;

    public ComponentService(IOllamaService ollamaService, ISignalRHostedService signalRConnectionState)
    {
        _ollamaService = ollamaService;
        _signalRConnectionState = signalRConnectionState;
    }

    public IEnumerable<Component> GetComponents()
    {
        yield return new Component
        {
            Name = "Ollama",
            Essential = true,
            CheckAsync = async _ =>
            {
                try
                {
                    var response = await _ollamaService.GetVersionAsync();
                    return new CheckResult
                    {
                        Success = true,
                        Message = $"Ollama with version {response} available."
                    };
                }
                catch (Exception e)
                {
                    return new CheckResult
                    {
                        Success = false,
                        Message = e.Message
                    };
                }
            }
        };

        yield return new Component
        {
            Name = "Server",
            Essential = true,
            CheckAsync = _ => Task.FromResult(new CheckResult
            {
                Success = _signalRConnectionState.State == HubConnectionState.Connected,
                Message = $"Server status is {_signalRConnectionState.State}."
            })
        };

    }
}