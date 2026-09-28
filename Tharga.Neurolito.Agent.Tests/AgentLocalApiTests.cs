using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tharga.Neurolito.Agent.Controllers;
using Tharga.Neurolito.Agent.Framework;

namespace Tharga.Neurolito.Agent.Tests;

/// <summary>
/// The agent's own HTTP API: no endpoint that acts on the machine answers unless configured to.
/// </summary>
/// <remarks>
/// It has no authentication and the acting endpoints are GETs, so any web page open in a browser on the
/// agent's machine could run prompts or remove models, and in the container anyone on the network could.
/// </remarks>
public class AgentLocalApiTests
{
    [Fact]
    public void ActionsAreOffByDefault()
    {
        Assert.False(new LocalApiOptions().AllowActions);
    }

    [Fact]
    public void AnActingEndpointIsNotFoundWhenActionsAreOff()
    {
        var context = Executing(new LocalApiOptions());

        new LocalActionAttribute().OnResourceExecuting(context);

        Assert.IsType<NotFoundResult>(context.Result);
    }

    [Fact]
    public void AnActingEndpointRunsWhenActionsAreOn()
    {
        var context = Executing(new LocalApiOptions { AllowActions = true });

        new LocalActionAttribute().OnResourceExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void WithNoConfigurationAtAllActionsStayOff()
    {
        var context = Executing(null);

        new LocalActionAttribute().OnResourceExecuting(context);

        Assert.IsType<NotFoundResult>(context.Result);
    }

    [Theory]
    [InlineData(typeof(PromptController), "Get")]
    [InlineData(typeof(EngineController), nameof(EngineController.InstallModel))]
    [InlineData(typeof(EngineController), nameof(EngineController.UninstallModel))]
    public void EveryEndpointThatActsOnTheMachineIsGated(Type controller, string action)
    {
        var method = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<LocalActionAttribute>());
    }

    [Theory]
    [InlineData(typeof(EngineController), nameof(EngineController.GetModels))]
    [InlineData(typeof(EngineController), nameof(EngineController.GetVersion))]
    [InlineData(typeof(StatusController), nameof(StatusController.GetStatus))]
    public void ReadOnlyEndpointsStayOpen(Type controller, string action)
    {
        // The install guide's "check it worked" step reads these.
        Assert.Null(controller.GetMethod(action)!.GetCustomAttribute<LocalActionAttribute>());
    }

    private static ResourceExecutingContext Executing(LocalApiOptions? options)
    {
        var services = new ServiceCollection();
        if (options != null) services.AddSingleton(Options.Create(options));

        var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());

        return new ResourceExecutingContext(action, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
    }
}
