using System.CommandLine;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI;

public static class PlumbobCommandBuilders
{
    internal static Command BuildCommand_InitLibrary()
    {
        Option<string> libraryPath = new("--path", "-p");
        return Build("init", "Initializes a mod library", parseResult =>
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
            
        }, options: [libraryPath]);
    }

    internal static Command BuildCommand_GetLibraryPath()
    {
        return Build("get-path", "Gets the current library path.", _ =>
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
    }
    
    internal static Command BuildCommand_SetLibraryPath()
    {
        var libPathArg = new Argument<string>("lib-path");
        return Build("set-path", "Selects the folder to use as the mod library.", parseResult => {
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
        }, arguments: [libPathArg]);
    }
    
    internal static Command Build(
        string commandName, string description, 
        Action<ParseResult> commandAction, 
        Option[]? options=null, 
        Argument[]? arguments=null)
    {
        var comm = new Command(commandName, description);
        options?.ForEach(comm.Add);
        arguments?.ForEach(comm.Add);
        comm.SetAction(commandAction);
        return comm;
    }

    internal static Command BuildCommand_ListLibrary()
    {
        var verboseOption = new Option<bool>("--verbose", "-v");
        return Build("list", "Lists all mods in the library.", parseResult => {
            bool verboseMode = parseResult.GetValue(verboseOption);
            if (verboseMode)
                PlumbobMsg.WriteUserMsg("Verbose mode enabled for listing mods.");
            
            var libReadTask = ServiceLocator.ResolveAsync<IAsyncModLibraryService>();

            int loadingAnimFrame = 0; //goes up to 3 then resets to 0 for: / - \ |
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

            if (verboseMode)
                PlumbobMsg.WriteUserMsg("Library loaded successfully.");
            
            var mods = libReadTask.Result.GetAllMods().ToList();
            PlumbobMsg.WriteUserMsg($"Mods in Library ({mods.Count}):");
            foreach (var mod in mods)
            {
                PlumbobMsg.WriteUserMsg(mod.ToString());
            }
        }, options: [verboseOption]);
    }

    internal static Command BuildCommand_CreateRig()
    {
        return Build("create", "Creates a rig", _ => { });
    }
    
    internal static Command BuildCommand_DeleteRig()
    {
        return Build("delete", "Deletes a rig", _ => { });
    }

    internal static Command BuildCommand_SelectRig()
    {
        return Build("select", "Selects a rig", _ => { });
    }

    internal static Command BuildCommand_AddModFolder()
    {
        return Build("add", "adds a new mod folder by copying it", _ => { });
    }

    internal static Command BuildCommand_InstallModArchive()
    {
        return Build("install", "installs a new mod from its archive/instructions", _ => { });
    }
}