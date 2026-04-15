using System.Text.Json.Serialization;
using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

public record ModEntry
{
    //updated from primary constructor syntax to the older and more stable properties + paramless ctor.
    //The former does not handle serialization well; primarily I think because it means
    //you can't create a "blank" instance or dynamically construct an immutable record in full
    //unless you have direct access to all of the properties being serialized.
    //This likely applies to all types, at least when working with System.Text.Json
    
    [JsonInclude, JsonPropertyName("rootPath")]
    public string RootPath { get; init; }

    [JsonInclude, JsonPropertyName("modMetadata")]
    public ModMetadata? ModMetadata { get; init; }

    public ModEntry() { }

    public ModEntry(string rootPath, ModMetadata? modMetadata)
    {
        RootPath = rootPath;
        ModMetadata = modMetadata;
    }

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