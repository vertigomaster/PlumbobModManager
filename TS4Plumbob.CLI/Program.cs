using TS4Plumbob.Core.DataModels;
using System.CommandLine;
using System.Diagnostics;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core;

namespace Plumbob.CLI;


class Program
{
    private static PlumbobKernel Core => PlumbobKernel.Instance;
    private static AppConfig Config => ServiceLocator.Resolve<AppConfig>();

    private static async Task<int> Main(string[] args)
    {
        int bootCode = await BootCore(args);
        if (bootCode != 0) return bootCode;
        WriteBootMessage();

        if (args.Length != 0) return await EnterImmediateMode(args);

        PlumbobMsg.WriteDebugInfo("No arguments provided. Initializing in interactive mode...");
        await EnterInteractiveMode();
        return 0;
    }

    private static async Task<int> BootCore(string[] args)
    {
        Debug.WriteLine("Booting up Plumbob Mod Manager CLI...");
        Debug.WriteLine("Current working directory: " + Directory.GetCurrentDirectory());

        Console.Title = "Plumbob Mod Manager";
        int startCode = await PlumbobKernel.StartInstance(args);
        if (startCode != 0) return startCode;

        return 0;
    }

    private static void WriteBootMessage()
    {
        string bootMsg = $"Plumbob Mod Manager CLI v{Config.ShortVersionString}";
        bootMsg = bootMsg.ToBorderedString(
            borderWidth: 3, 
            horizontalSpacing: 3, 
            borderHeight: 1, 
            borderChar: '=');
        PlumbobMsg.WriteUserMsg(bootMsg, ConsoleColor.Green);
    }

    //TODO: build REPL template for later projects
    private static async Task<int> EnterImmediateMode(string[] args)
    {
        Core.LoggingMode = PlumbobKernel.LogMode.File;
        RootCommand rootCommand = PlumbobCmd.BuildCommandTree();
        var parseResult = rootCommand.Parse(args);
        int result = await parseResult.InvokeAsync();
        
        if(result != 0) return result;

        await ShutdownCore();
        
        return 0;
    }

    private static async Task EnterInteractiveMode()
    {
        Core.LoggingMode = PlumbobKernel.LogMode.Console | PlumbobKernel.LogMode.File;
        
        RootCommand rootCommand = PlumbobCmd.BuildCommandTree();
        
        //introduction
        PlumbobMsg.WriteUserMsg("Welcome to the Plumbob Mod Manager CLI's interactive mode!");
        PlumbobMsg.WriteUserMsg("Type '--help' or '-h' to see a list of available commands.");
        PlumbobMsg.WriteUserMsg("Type 'exit' to exit the program.");
        
        //start up REPL
        while (true) //TODO: add some flag or guard to this
        {
            Console.Write($"\n[{Config.ShortAppName} - {Config.ShortVersionString}] >> ");
            
            var input = Console.ReadLine()?.Trim();
            if(string.IsNullOrWhiteSpace(input)) continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                PlumbobMsg.WriteDebugInfo("Exiting Interactive Mode...");
                await ShutdownCore();
                break;
            }

            try
            {
                var parseResult = rootCommand.Parse(input);
                await parseResult.InvokeAsync();
            }
            catch(Exception e) //TODO catch specific exceptions
            {
                PlumbobMsg.WriteUserError($"Error parsing command \"{input}\": {e}");
                PlumbobMsg.WriteUserError("Invalid command. Type 'help' for a list of available commands.");
            }
        }
    }

    private static async Task ShutdownCore()
    {
        ConsoleLog.Log("Shutting down Plumbob Mod Manager CLI...");
        await Core.Shutdown();
        ConsoleLog.Log("Plumbob Mod Manager CLI shutdown complete.");
        //TODO: anything else?
    }
}