using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels.IdTypes;

namespace TS4Plumbob.Core.DataModels;

public record ModEntry(
    ModEntryId Id,
    string RootPath,
    ModMetadata? ModMetadata
)
{
    public static ModEntry CreateNewUnique(string rootPath, ModMetadata? modMetadata = null)
    {
        return new ModEntry(Guid.NewGuid(), rootPath, modMetadata);
    }
    
    private bool _hasMetadata;
    public bool HasMetadata => _hasMetadata;
    
    private AppConfig _AppConfig => ServiceLocator.Resolve<AppConfig>();
    
    public string RelPath => RootPath.Replace(_AppConfig.UserSettings.ModLibraryPath, "");
    public string HumanReadableIdentifier => ModMetadata?.Name ?? $"UNNAMED ({RelPath})";
    
    public bool ExistsOnDisk() 
    {
        return Directory.Exists(RootPath);
    }
}