using System.CommandLine;
using System.Reflection;
using IDEK.Tools.ConsoleCommander;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

namespace Plumbob.CLI;

public static class PlumbobCmd
{
    public static RootCommand BuildCommandTree()
    {
        var r = CommandRegistry.Build(Assembly.GetEntryAssembly()!);
        r.Description = "Plumbob Mod Manager CLI (Command Line Interface) " +
            "for The Sims 4. Directly runs PMM commands.";
        return r;
    }
}

[Obsolete("Deprecated; now using ConsoleCommander.CommandRegistry")]
public static class PlumbobCmd_Old
{
    public static RootCommand BuildCommandTree()
    {
        return CommandRegistry.Build(Assembly.GetEntryAssembly()!);
        
        RootCommand rootCommand = new(
            "Plumbob Mod Manager CLI (Command Line Interface) for The Sims 4. Directly runs PMM commands.")
        {
            Subcommands = {
                _BuildCommands_Test(),
                // _BuildCommands_Core(),
                _BuildCommands_Library(),
                _BuildCommands_Rig(),
                _BuildCommands_Mod()
            }
        };
        
        return rootCommand;
    }

    [Obsolete("This is a placeholder for the future. It is not yet implemented.")]
    private static Command _BuildCommands_Core()
    {
        var initLibraryCommand = PlumbobCommandBuilders.BuildCommand_InitLibrary();


        return initLibraryCommand;
    }

    private static Command _BuildCommands_Test()
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

    private static Command _BuildCommands_Library()
    {
        return new("library", "Subcommand entry associated with the mod library.") {
            Subcommands = {
                PlumbobCommandBuilders.BuildCommand_InitLibrary(),
                PlumbobCommandBuilders.BuildCommand_SetLibraryPath(),
                PlumbobCommandBuilders.BuildCommand_ListLibrary(),
                PlumbobCommandBuilders.BuildCommand_GetLibraryPath()
            }
        };
    }

    private static Command _BuildCommands_Rig()
    {
        return new Command("rig", "Subcommand entry associated with whole mod rigs.") {
            Subcommands = {
                PlumbobCommandBuilders.BuildCommand_CreateRig(),
                PlumbobCommandBuilders.BuildCommand_SelectRig(),
                PlumbobCommandBuilders.BuildCommand_DeleteRig()
            }
        };
    }

    private static Command _BuildCommands_Mod()
    {
        Command modMetaCommand = new("mod", "Subcommand entry associated with individual mods.");

        modMetaCommand.Subcommands.Add(PlumbobCommandBuilders.BuildCommand_InstallModArchive());
        modMetaCommand.Subcommands.Add(PlumbobCommandBuilders.BuildCommand_AddModFolder());
        
        return modMetaCommand;
    }
}