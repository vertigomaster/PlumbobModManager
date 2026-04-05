using TS4Plumbob.Core.DataModels;
using System.Text.Json.Serialization;
using System.CommandLine;

namespace Plumbob.CLI;

class Program
{
    private static bool persistentMode = false;
    
    static async Task<int> Main(string[] args)
    {
        Console.Title = "Plumbob Mod Manager";
        
        //if no args, prompt for a line, break it into tokens, and run that?
        
        RootCommand rootCommand = new("Plumbob Mod Manager CLI (Command Line Interface) for The Sims 4. Directly runs PMM commands.");

        var testCommand = BuildTestCommands();
        rootCommand.Subcommands.Add(testCommand);
        
        var rigCommand = BuildRigCommands();
        rootCommand.Subcommands.Add(rigCommand);

        var modMetaCommand = BuildModCommands();
        rootCommand.Subcommands.Add(modMetaCommand);

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
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

    static void SaveConfigFile()
    {
        
    }

    static void LoadConfigFile()
    {
        
    }
}