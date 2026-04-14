using IDEK.Tools.Logging;

namespace Plumbob.Core.Utils;

public static class PlumbobMsg
{
    /// <summary>
    /// Writes an error message to the console in red color with a preceding error symbol.
    /// </summary>
    /// <param name="message">The error message to be displayed.</param>
    public static void WriteUserError(string message)
    {
        ConsoleLog.LogError(message);
    }

    /// <summary>
    /// Writes a warning message to the console with a yellow font color,
    /// prefixed with a warning symbol.
    /// </summary>
    /// <param name="message">The warning message to be displayed.</param>
    public static void WriteDebugWarning(string message)
    {
        // Console.ForegroundColor = ConsoleColor.Yellow;
        // Console.WriteLine("⚠ " +  message);
        // Console.ResetColor();
        ConsoleLog.LogWarning(message);
    }

    /// <summary>
    /// Writes an informational message to the console in cyan color with a preceding informational symbol.
    /// </summary>
    /// <param name="message">The informational message to be displayed on the console.</param>
    public static void WriteDebugInfo(string message)
    {
        // Console.ForegroundColor = ConsoleColor.Cyan;
        // Console.WriteLine("ℹ " + message);
        // Console.ResetColor();
        ConsoleLog.Log(message);
    }

    /// <summary>
    /// Writes a specified user-intended message to the console.
    /// </summary>
    /// <param name="message">The message to be written to the console.</param>
    public static void WriteUserMsg(string message, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}