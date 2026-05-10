using IDEK.Tools.ConsoleCommander;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI.Commands;

public static class LibraryCommands
{
    [Command("lib path set", Description = "Initializes a mod library")]
    public static void SetLibraryPath(string path)
    {
        if(path.IsNullOrWhitespace())
        {
            PlumbobMsg.WriteUserError("Library path cannot be empty or whitespace.");
            // // Use a standard platform-agnostic starting directory
            // path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        
        var config = ServiceLocator.Resolve<AppConfig>() ?? 
            throw new InvalidOperationException("AppConfig not found");
        config.UserSettings.ModLibraryPath = path;
        config.SaveToDisk();
    }
    
    [Command("lib path get", Description = "Gets the current library path.")]
    public static void GetLibraryPath()
    {
        var appConfig = ServiceLocator.Resolve<AppConfig>();
        if (appConfig == null)
        {
            PlumbobMsg.WriteUserError("Error: Failed to resolve AppConfig service - cannot get library path.");
            return;
        }

        var libPath = appConfig.UserSettings.ModLibraryPath;
        PlumbobMsg.WriteUserMsg($"Current library path: {libPath}");
    }

    [Command("lib list", Description = "Lists all mods in the library.")]
    public static void ListLibrary(bool verboseMode=false)
    {
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
            PlumbobMsg.WriteDebugError("Library loading task failed. Potentially with exception: " +
                libReadTask.Exception);
            return;
        }

        if (libReadTask.Result == null)
        {
            PlumbobMsg.WriteUserError("Failed to load library");
            PlumbobMsg.WriteDebugError(
                "Library loading task completed successfully " +
                "but result is null.");
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
    }
}