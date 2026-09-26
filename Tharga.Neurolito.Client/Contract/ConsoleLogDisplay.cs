namespace Tharga.Neurolito.Client.Contract;

/// <summary>How a console line from an agent is shown: a routine connection drop is one readable line.</summary>
/// <param name="Message">The line to show.</param>
/// <param name="Detail">The full exception, for anyone who wants it, or null.</param>
/// <param name="IsConnectionDrop">Whether this is the agent losing its connection to the server, which happens on every server deploy.</param>
public record ConsoleLogDisplay(string Message, string Detail, bool IsConnectionDrop)
{
    private static readonly string[] _connectionExceptions =
    [
        "System.Net.WebSockets.WebSocketException",
        "System.IO.IOException",
        "System.Net.Http.HttpRequestException",
        "System.TimeoutException",
        "System.Threading.Tasks.TaskCanceledException",
        "System.OperationCanceledException"
    ];

    /// <summary>Reads one entry.</summary>
    /// <remarks>
    /// Tharga.Communication logs "SignalR reconnecting" with the exception attached, so every server deploy
    /// wrote a full stack trace into each agent's console, where it read as a failure (2026-09-25). It is the
    /// server going away and coming back, and the agent reconnects by itself.
    /// </remarks>
    public static ConsoleLogDisplay Of(string message, string exception)
    {
        if (string.IsNullOrWhiteSpace(exception)) return new ConsoleLogDisplay(message, null, false);

        var isConnection = _connectionExceptions.Any(x => exception.TrimStart().StartsWith(x, StringComparison.Ordinal));
        var isSignalR = message != null && message.Contains("SignalR", StringComparison.OrdinalIgnoreCase);
        if (!isConnection || !isSignalR) return new ConsoleLogDisplay(message, exception, false);

        return new ConsoleLogDisplay($"{message}: the connection to the server was lost, reconnecting. ({FirstLine(exception)})", exception, true);
    }

    /// <summary>The exception's first line, which names its type and message.</summary>
    public static string FirstLine(string exception)
    {
        var line = exception.Split('\n', 2)[0].Trim();
        var colon = line.IndexOf(": ", StringComparison.Ordinal);
        return colon > 0 ? line[(colon + 2)..] : line;
    }
}
