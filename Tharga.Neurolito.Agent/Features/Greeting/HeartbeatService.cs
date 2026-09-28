using Tharga.Communication.Client.Communication;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Features.Greeting;

public class HeartbeatService : BackgroundService
{
    private readonly IClientCommunication _clientCommunication;

    public HeartbeatService(IClientCommunication clientCommunication)
    {
        _clientCommunication = clientCommunication;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cointer = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_clientCommunication.IsConnected)
            {
                await _clientCommunication.PostAsync(new HeartbeatMessage { Counter = cointer++ });
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
        }
    }
}