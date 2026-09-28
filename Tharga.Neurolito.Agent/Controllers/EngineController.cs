using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Tharga.Communication.Contract;
using Tharga.Neurolito.Agent.Features.Ollama;
using Tharga.Neurolito.Agent.Framework;
using Tharga.Neurolito.Client.Contract;

namespace Tharga.Neurolito.Agent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EngineController : ControllerBase
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<EngineController> _logger;

    public EngineController(IOllamaService ollamaService, ILogger<EngineController> logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    [HttpGet("model")]
    public async Task<IActionResult> GetModels(CancellationToken cancellationToken)
    {
        var models = await _ollamaService.GetModelsAsync(cancellationToken).ToArrayAsync(cancellationToken);
        return Ok(new ModelResponse { Models = models });
    }

    [HttpGet("model/install/{model}")]
    [LocalAction]
    public async Task InstallModel(string model = Constants.DefaultModel, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(model)) model = Uri.UnescapeDataString(model);

        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        await foreach (var progress in _ollamaService.InstallModelAsync(model, false, cancellationToken))
        {
            var json = JsonSerializer.Serialize(progress);

            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            _logger.LogDebug($"data: {json}");
        }
    }

    [HttpGet("model/uninstall/{model}")]
    [LocalAction]
    public async Task<IActionResult> UninstallModel(string model = Constants.DefaultModel, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(model)) model = Uri.UnescapeDataString(model);

        await _ollamaService.UninstallModelAsync(model, false, cancellationToken);
        return Ok();
    }

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion(CancellationToken cancellationToken)
    {
        var response = await _ollamaService.GetVersionAsync(cancellationToken);
        return Ok(response);
    }
}