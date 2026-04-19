using System.CommandLine;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI;

public static class PlumbobCmd
{
    public static RootCommand BuildCommandTree()
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

    private static Command BuildCoreCommands()
    {
        Command initLibraryCommand = new("init-library", "Initializes a mod library");
        Option<string> libraryPath = new("--path", "-p");
        initLibraryCommand.Add(libraryPath);
        
        initLibraryCommand.SetAction(parseResult =>
        {
            string folderPath = parseResult.GetValue(libraryPath);
            if (string.IsNullOrEmpty(folderPath))
            {
                // Use a standard platform-agnostic starting directory
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            // // Open a folder selection dialog
            // using var folderDialog = new FolderBrowserDialog
            // {
            //     Description = "Select a folder for initializing the mod library",
            //     UseDescriptionForTitle = true,
            //     SelectedPath = folderPath
            // };
            //
            // var result = folderDialog.ShowDialog();
            //
            // if (result == DialogResult.OK && !string.IsNullOrEmpty(folderDialog.SelectedPath))
            // {
            //     PlumbobMsg.WriteUserMsg($"Selected folder: {folderDialog.SelectedPath}");
            // }
            // else
            // {
            //     PlumbobMsg.WriteUserMsg("No folder selected or operation cancelled.");
            // }
            
        });
        
        
        return initLibraryCommand;
    }

    private static Command BuildTestCommands()
    {
        Option<bool> example = new("--example", "-e");
        Option<bool> fartMode = new("--fart-mode", "-f");
        
        Command testCommand = new("test", "testing out subcommands") {
            example, fartMode
        };
        
        testCommand.SetAction(parseResult => {
            bool isExample = parseResult.GetValue(example);
            bool isFartMode = parseResult.GetValue(fartMode);
            
            if(isExample) PlumbobMsg.WriteUserMsg("Example option is enabled!");
            
            if(isFartMode) PlumbobMsg.WriteUserMsg("PBBBPBBBPBPTTTTT 💨");

            if (!isFartMode && !isExample)
            {
                PlumbobMsg.WriteUserMsg("No options enabled.");
            }
        });
        
        return testCommand;
    }

    private static Command BuildLibraryCommands()
    {
        Command libraryMetaCommand = new("library", "Commands associated with the mod library.");
        
        Command selectLibraryCommand = new("select", "Selects the folder to use as the mod library.");
        selectLibraryCommand.Add(new Argument<string>("lib-path"));
        selectLibraryCommand.SetAction(parseResult => {
            string libPath = parseResult.GetValue<string>("lib-path") ?? string.Empty;
            PlumbobMsg.WriteUserMsg($"Selected library path: {libPath}");
            
            if (string.IsNullOrWhiteSpace(libPath))
            {
                PlumbobMsg.WriteUserError("Library path cannot be empty or whitespace.");
                return;
            }
            
            var appConfig = ServiceLocator.Resolve<AppConfig>();
            appConfig.UserSettings.ModLibraryPath = libPath;
            appConfig.SaveToDisk();
        });
        
        Command listLibraryCommand = new("list", "Lists all mods in the library.");
        
        libraryMetaCommand.Subcommands.Add(selectLibraryCommand);
        libraryMetaCommand.Subcommands.Add(listLibraryCommand);
        return libraryMetaCommand;
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

        Command installModArchiveCommand = new("install", 
            "installs a new mod from its archive/instructions");
        Command addModFolderCommand = new("add", "adds a new mod folder by copying it");

        modMetaCommand.Subcommands.Add(installModArchiveCommand);
        modMetaCommand.Subcommands.Add(addModFolderCommand);
        return modMetaCommand;
    }
}