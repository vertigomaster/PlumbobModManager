using System.CommandLine;
using IDEK.Tools.ConsoleCommander;
using Plumbob.Core.Utils;

namespace Plumbob.CLI.Commands;

public static class TestCommands
{
    [Command("test", Description = "Test command for CLI")]
    public static void Test(
        ParseResult parseResult, 
        bool example=false, 
        bool fartMode=false)
    {
        if (example) PlumbobMsg.WriteUserMsg("Example option is enabled!");

        if (fartMode) PlumbobMsg.WriteUserMsg("PBBBPBBBPBPTTTTT 💨");

        if (!fartMode && !example)
        {
            PlumbobMsg.WriteUserMsg("No options enabled.");
        }
    }
}