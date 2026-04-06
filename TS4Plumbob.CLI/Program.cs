using TS4Plumbob.Core.DataModels;
using System.Text.Json.Serialization;
using System.CommandLine;
using System.Diagnostics;
using System.Text;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;

namespace Plumbob.CLI;

class Program
{
    private static PlumbobKernel Core { get; set; } = new();

    private static AppConfig Config => ServiceLocator.Resolve<AppConfig>();

    private static async Task<int> Main(string[] args)
    {
        int bootCode = await BootCore(args);
        if (bootCode != 0) return bootCode;

        if (args.Length != 0) return await EnterImmediateMode(args);
        
        WriteInfo("No arguments provided. Initializing in interactive mode...");
        await EnterInteractiveMode();
        return 0;
    }

    private static async Task<int> BootCore(string[] args)
    {
        Debug.WriteLine("Booting up Plumbob Mod Manager CLI...");
        Debug.WriteLine("Current working directory: " + Directory.GetCurrentDirectory());
        
        Console.Title = "Plumbob Mod Manager";
        int startCode = await Core.Start(args);
        if (startCode != 0) return startCode;
        
        WriteBootMessage();
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
        WriteUserMsg(bootMsg);
    }

    //TODO: build REPL template for later projects
    private static async Task<int> EnterImmediateMode(string[] args)
    {
        Core.LoggingMode = PlumbobKernel.LogMode.File;
        RootCommand rootCommand = BuildCommandTree();
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static async Task EnterInteractiveMode()
    {
        Core.LoggingMode = PlumbobKernel.LogMode.Console | PlumbobKernel.LogMode.File;
        
        RootCommand rootCommand = BuildCommandTree();
        
        //introduction
        WriteUserMsg("Welcome to the Plumbob Mod Manager CLI's interactive mode!");
        WriteUserMsg("Type 'help' to see a list of available commands.");
        WriteUserMsg("Type 'exit' to exit the program.");
        
        //start up REPL
        while (true) //TODO: add some flag or guard to this
        {
            Console.Write($"\n[{Config.ShortAppName} {Config.ShortVersionString}] >> ");
            
            var input = Console.ReadLine()?.Trim();
            if(string.IsNullOrWhiteSpace(input)) continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                WriteInfo("Exiting Interactive Mode...");
                break;
            }

            try
            {
                var parseResult = rootCommand.Parse(input);
                await parseResult.InvokeAsync();
            }
            catch(Exception e) //TODO catch specific exceptions
            {
                WriteError($"Error parsing command \"{input}\": {e}");
                WriteError("Invalid command. Type 'help' for a list of available commands.");
            }
        }
    }

    /// <summary>
    /// Triggers a manual save of the config file
    /// </summary>
    static void SaveConfigFile()
    {
        Config.SaveToDisk();
    }

    /// <summary>
    /// Triggers a manual re-load of the config file (kernel should automatically load it when it is started up)
    /// </summary>
    static void ReloadConfigFile()
    {
        //this should rerun the jumpstarter bounded to the type.
        ServiceLocator.ReregisterBoundService<AppConfig>();
    }

    #region Console Write Helpers

    /// <summary>
    /// Writes an error message to the console in red color with a preceding error symbol.
    /// </summary>
    /// <param name="message">The error message to be displayed.</param>
    static void WriteError(string message)
    {
        ConsoleLog.LogError(message);
    }

    /// <summary>
    /// Writes a warning message to the console with a yellow font color,
    /// prefixed with a warning symbol.
    /// </summary>
    /// <param name="message">The warning message to be displayed.</param>
    static void WriteWarning(string message)
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
    static void WriteInfo(string message)
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
    static void WriteUserMsg(string message, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    #endregion

    //TODO: move to partial class?
    #region Command Tree

    private static RootCommand BuildCommandTree()
    {
        RootCommand rootCommand = new(
            "Plumbob Mod Manager CLI (Command Line Interface) for The Sims 4. Directly runs PMM commands.");

        var testCommand = BuildTestCommands();
        rootCommand.Subcommands.Add(testCommand);

        var rigCommand = BuildRigCommands();
        rootCommand.Subcommands.Add(rigCommand);

        var modMetaCommand = BuildModCommands();
        rootCommand.Subcommands.Add(modMetaCommand);
        
        return rootCommand;
    }

    private static Command BuildTestCommands()
    {
        Command testCommand = new("test", "testing out subcommands")
        {
            new Option<bool>("--example"),
            new Option<bool>("--fart-mode")
        };
        return testCommand;
    }

    private static Command BuildRigCommands()
    {
        Command rigMetaCommand = new("rig", "Commands associated with whole mod rigs.");
        
        Command createRigCommand = new("create", "Creates a rig");
        Command selectRigCommand = new("select", "selects a rig");
        Command deleteRigCommand = new("delete", "Deletes a rig");
        
        rigMetaCommand.Subcommands.Add(createRigCommand);
        rigMetaCommand.Subcommands.Add(selectRigCommand);
        rigMetaCommand.Subcommands.Add(deleteRigCommand);
        return rigMetaCommand;
    }

    private static Command BuildModCommands()
    {
        Command modMetaCommand = new("mod", "mod meta-command");

        Command addModArchiveCommand = new("add-mod", "adds a new mod archive");

        modMetaCommand.Subcommands.Add(addModArchiveCommand);
        return modMetaCommand;
    }

    #endregion
}