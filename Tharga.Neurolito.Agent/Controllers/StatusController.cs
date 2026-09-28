using Microsoft.AspNetCore.Mvc;
using Tharga.Communication.Client;
using Tharga.Neurolito.Agent.Features.Ollama;

namespace Tharga.Neurolito.Agent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly ISignalRHostedService _signalRConnectionState;
    private readonly IOllamaService _ollamaService;

    public StatusController(ISignalRHostedService signalRConnectionState, IOllamaService ollamaService)
    {
        _signalRConnectionState = signalRConnectionState;
        _ollamaService = ollamaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var state = _signalRConnectionState.State;
        return Ok($"{state}");
    }

    [HttpGet("capability")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var response = await _ollamaService.BuildCapabilityAsync();
        return Ok(response);
    }

}