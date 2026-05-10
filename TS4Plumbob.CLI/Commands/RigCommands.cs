using IDEK.Tools.ConsoleCommander;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI.Commands;

public static class RigCommands
{
    // [Command("rig create", Description = "Creates a new rig")]
    // [Command("rig new", Description = "Creates a new rig")]
    // public static void CreateRig()
    // {
    //     //how do you create a new rig?
    // }

    [Command("rig list", Description = "Lists active rig contents")]
    public static void GetVisibleModsInActiveRig(
        bool showFolder=false, 
        bool verbose=false
        // string targetRig=""
        )
    {
        var lib = ServiceLocator.ResolveAsync<IAsyncModLibraryService>().Result ?? 
            throw new InvalidOperationException("Library not found");
        
        //TODO: enable getting mods visible from a specific rig
        IEnumerable<ModEntry> visModEnumer = lib.GetVisibleMods();
        var visibleMods = visModEnumer as ModEntry[] ?? visModEnumer.ToArray();

        if (verbose)
        {
            PlumbobMsg.WriteUserMsg($"Note: mods not in the active rig are NOT visible!");
            PlumbobMsg.WriteUserMsg($"Found {visibleMods.Length} mods");
        }
        
        foreach (var mod in visibleMods)
        {
            if (showFolder)
            {
                Console.WriteLine($"{mod.Slug} ({mod.FolderName})");
            }
            else
            {
                Console.WriteLine(mod.Slug);
            }
        }
    }
}