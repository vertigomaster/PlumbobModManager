using System.CommandLine;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI;

public static class PlumbobCommandBuilders
{
    internal static Command BuildCommand_InitLibrary()
    {
        Command initLibraryCommand = new("init", "Initializes a mod library");
        Option<string> libraryPath = new("--path", "-p");
        initLibraryCommand.Add(libraryPath);
        
        initLibraryCommand.SetAction(parseResult =>
        {
            string? folderPath = parseResult.GetValue(libraryPath);
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

    internal static Command BuildCommand_GetLibraryPath()
    {
        var cmd = new Command("get-path", "Gets the current library path.");
        cmd.SetAction(_ =>
        {
            var appConfig = ServiceLocator.Resolve<AppConfig>();
            if (appConfig == null)
            {
                PlumbobMsg.WriteUserError("Error: Failed to resolve AppConfig service - cannot get library path.");
                return;
            }
            var libPath = appConfig.UserSettings.ModLibraryPath;
            PlumbobMsg.WriteUserMsg($"Current library path: {libPath}");
        });
        return cmd;
    }
    
    internal static Command BuildCommand_SetLibraryPath()
    {
        Command selectLibraryCommand = new("set-path", "Selects the folder to use as the mod library.");
        var libPathArg = new Argument<string>("lib-path");
        selectLibraryCommand.Add(libPathArg);
        
        selectLibraryCommand.SetAction(parseResult => {
            string libPath = parseResult.GetValue(libPathArg) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(libPath))
            {
                PlumbobMsg.WriteUserError("Library path cannot be empty or whitespace.");
                return;
            }
            
            PlumbobMsg.WriteUserMsg($"Selected library path: {libPath}");
            
            var appConfig = ServiceLocator.Resolve<AppConfig>();
            if (appConfig == null)
            {
                PlumbobMsg.WriteUserError("Error: Failed to resolve AppConfig service - cannot save updated library path.");
                return;
            }
            appConfig.UserSettings.ModLibraryPath = libPath;
            appConfig.SaveToDisk();
        });
        
        return selectLibraryCommand;
    }

    internal static Command BuildCommand_ListLibrary()
    {
        var listLibraryCommand = new Command("list", "Lists all mods in the library.");
        var verboseOption = new Option<bool>("--verbose", "-v");
        listLibraryCommand.Add(verboseOption);
        
        listLibraryCommand.SetAction(parseResult => {
            bool verboseMode = parseResult.GetValue(verboseOption);
            if (verboseMode)
            {
                PlumbobMsg.WriteUserMsg("Verbose mode enabled for listing mods.");
            }
            
            var libReadTask = ServiceLocator.ResolveAsync<IAsyncModLibraryService>();

            int loadingAnimFrame = 0; //goes up to 3 then resets to 0 for: / - \ |
            string loadingFrames = "/-\\|";
            while (!libReadTask.IsCompleted)
            {
                PlumbobMsg.DrawASCIILoadingSpinner("Loading library...", loadingAnimFrame++);
            }

            if (!libReadTask.IsCompletedSuccessfully)
            {
                PlumbobMsg.WriteUserError("Failed to load library");
                PlumbobMsg.WriteDebugError("Library loading task failed. Potentially with exception: " + libReadTask.Exception);
                return;
            }
            
            if(libReadTask.Result == null)
            {
                PlumbobMsg.WriteUserError("Failed to load library");
                PlumbobMsg.WriteDebugError("Library loading task completed successfully but result is null.");
                return;
            }
            
            PlumbobMsg.WriteUserMsg("Library loaded successfully.");
            
            var mods = libReadTask.Result.GetAllMods().ToList();
            PlumbobMsg.WriteUserMsg($"Mods in Library ({mods.Count}):");
            foreach (var mod in mods)
            {
                PlumbobMsg.WriteUserMsg(mod.ToString());
            }
        });
        
        return listLibraryCommand;
    }

    internal static Command BuildCommand_CreateRig()
    {
        Command createRigCommand = new("create", "Creates a rig");
        return createRigCommand;
    }
    
    internal static Command BuildCommand_DeleteRig()
    {
        Command deleteRigCommand = new("delete", "Deletes a rig");
        return deleteRigCommand;
    }

    internal static Command BuildCommand_SelectRig()
    {
        Command selectRigCommand = new("select", "Selects a rig");
        return selectRigCommand;
    }

    internal static Command BuildCommand_AddModFolder()
    {
        Command addModFolderCommand = new("add", "adds a new mod folder by copying it");
        return addModFolderCommand;
    }

    internal static Command BuildCommand_InstallModArchive()
    {
        Command installModArchiveCommand = new("install", 
            "installs a new mod from its archive/instructions");
        return installModArchiveCommand;
    }
}