using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

public record ModEntry(
    string RootPath,
    ModMetadata? ModMetadata
)
{
    public static ModEntry CreateNewUnique(string rootPath, ModMetadata? modMetadata = null)
    {
        return new ModEntry(rootPath, modMetadata);
    }
    
    private bool _hasMetadata;
    public bool HasMetadata => _hasMetadata;
    
    private AppConfig _AppConfig => ServiceLocator.Resolve<AppConfig>();
    
    public string RelPath
    {
        get
        {
            string? libPath = _AppConfig?.UserSettings?.ModLibraryPath;
            return libPath is null ? RootPath : RootPath.Replace(libPath, "");
        }
    }

    public string HumanReadableIdentifier => ModMetadata?.Name ?? $"UNNAMED ({RelPath})";
    
    public ModEntrySlug Slug {get; set;}
    
    /// <summary>
    /// A back reference to the logical mod object this is an entry for.
    /// </summary>
    public Mod? ModConcept { get; set; }

    public bool ExistsOnDisk() 
    {
        return Directory.Exists(RootPath);
    }
}