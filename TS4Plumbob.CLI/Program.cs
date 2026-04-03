using TS4Plumbob.Core.DataModels;
using System.Text.Json.Serialization;
using System.CommandLine;

namespace Plumbob.CLI;

class Program
{
    string AppFolder => AppContext.BaseDirectory;
    string AppConfigDir => Path.Combine(AppFolder, "config");
    
    private static bool persistentMode = false;
    static async Task<int> Main(string[] args)
    {
        Console.Title = "Plumbob Mod Manager";
        
        //if no args, prompt for a line, break it into tokens, and run that?
        
        RootCommand rootCommand = new("Plumbob Mod Manager CLI (Command Line Interface) for The Sims 4. Directly runs PMM commands.");

        Command testCommand = new("test", "testing out subcommands")
        {
            new Option<bool>("--example"),
            new Option<bool>("--fart-mode")
        };
        rootCommand.Subcommands.Add(testCommand);

        
        
        Command rigCommand = new("rig", "Commands associated with whole mod rigs.");
        rootCommand.Subcommands.Add(rigCommand);
        
        Command createRigCommand = new("create", "Creates a rig");
        Command selectRigCommand = new("select", "selects a rig");
        Command deleteRigCommand = new("delete", "Deletes a rig");
        
        rigCommand.Subcommands.Add(createRigCommand);
        rigCommand.Subcommands.Add(selectRigCommand);
        rigCommand.Subcommands.Add(deleteRigCommand);
        
        Command addModArchiveCommand = new("add-mod", "adds a new mod archive");
        rootCommand.Subcommands.Add(addModArchiveCommand);

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    static void SaveConfigFile()
    {
        
    }

    static void LoadConfigFile()
    {
        
    }
}