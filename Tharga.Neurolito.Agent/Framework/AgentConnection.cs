using ClientOptions = Tharga.Communication.Client.CommunicationOptions;

namespace Tharga.Neurolito.Agent.Framework;

/// <summary>
/// How the agent's hub connection is configured.
/// </summary>
/// <remarks>
/// <c>AddThargaCommunicationClient</c> in Tharga.Communication 1.1.1 copies only <c>ServerAddress</c>
/// and <c>Pattern</c> from the <c>Tharga:Communication</c> section. <c>ApiKey</c> is read from the
/// options callback alone, so a key in configuration — <c>appsettings.Agent.json</c> written by the
/// Chocolatey install, or <c>Tharga__Communication__ApiKey</c> passed to a container — is silently
/// dropped unless it is carried across here.
/// </remarks>
internal static class AgentConnection
{
    /// <summary>Configuration key holding the team credential the agent connects with.</summary>
    public const string ApiKeySetting = "Tharga:Communication:ApiKey";

    /// <summary>Hub path on the server.</summary>
    public const string HubPattern = "agentHub";

    /// <summary>Applies the agent's connection settings to the communication options.</summary>
    public static void Configure(ClientOptions options, IConfiguration configuration)
    {
        options.Pattern = HubPattern;

        var apiKey = configuration[ApiKeySetting];
        if (!string.IsNullOrWhiteSpace(apiKey)) options.ApiKey = apiKey;
    }
}
