using System.Reflection;

namespace Tharga.Neurolito.Client;

/// <summary>
/// How this package names itself to the server, so the server can tell which client versions are in use.
/// </summary>
/// <remarks>
/// Sent as the <c>User-Agent</c> of every request, in the standard <c>product/version</c> form. Before
/// this, a request carried no hint of which package sent it, so there was no way to tell how old a
/// client the server still had to support.
/// </remarks>
public static class ClientUserAgent
{
    /// <summary>The product token this package sends.</summary>
    public const string Product = "Tharga.Neurolito.Client";

    /// <summary>This package's version, including the commit it was built from when known.</summary>
    public static string Version { get; } = typeof(ClientUserAgent).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                                            ?? typeof(ClientUserAgent).Assembly.GetName().Version?.ToString()
                                            ?? "unknown";

    /// <summary>The full <c>User-Agent</c> value.</summary>
    public static string Value => $"{Product}/{Version}";
}
