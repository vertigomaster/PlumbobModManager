namespace TS4Plumbob.Core.DataModels;

public record ModRigSnapshot
{
    public ModEntry[] OrderedInstallList { get; init; }

    public ModRigSnapshot() { }

    public ModRigSnapshot(ModEntry[] orderedInstallList)
    {
        OrderedInstallList = orderedInstallList;
    }
}