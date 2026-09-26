using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Tharga.Neurolito.Agent.Framework;

/// <summary>
/// What the agent's own HTTP API may do.
/// </summary>
/// <remarks>
/// The server drives an agent over the SignalR hub, never through this API; it exists for development
/// and diagnostics. It has no authentication, and the acting endpoints are GETs, so if they were on
/// anyone who could reach the port could run prompts and install or remove models - on every interface
/// in the container, and from any web page open in a browser on the machine. They are off by default.
/// </remarks>
public record LocalApiOptions
{
    public const string SectionName = "LocalApi";

    /// <summary>
    /// Whether the endpoints that act on the machine answer - running a prompt, installing and
    /// uninstalling a model. Off by default; <c>appsettings.Development.json</c> turns it on.
    /// </summary>
    public bool AllowActions { get; init; }
}

/// <summary>
/// Marks an endpoint that acts on the machine. It answers 404 unless
/// <see cref="LocalApiOptions.AllowActions"/> is on.
/// </summary>
/// <remarks>
/// 404 rather than 403, so an agent in production does not advertise an endpoint it will not serve.
/// Read-only endpoints - status, capability, models, version, health, swagger - are not marked: the
/// install guide's "check it worked" step reads them.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class LocalActionAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var options = context.HttpContext.RequestServices.GetService<IOptions<LocalApiOptions>>()?.Value;
        if (options?.AllowActions == true) return;

        context.Result = new NotFoundResult();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
