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
    
    // [Obsolete("Realizing that we don't need these; we have the slugs and a determinsitic library file structure")]
    // [JsonInclude, JsonPropertyName("rootPath")]
    // public string RootPath { get; init; }

    #region Core Properties

    [JsonInclude, JsonPropertyName("modMetadata")]
    public ModMetadata ModMetadata { get; init; }

    /// <summary>
    /// A back reference to the logical mod object this is an entry for.
    /// </summary>
    public Mod ModConcept { get; init; }

    #endregion

    #region Computed Properties

    private AppConfig _AppConfig => ServiceLocator.Resolve<AppConfig>() ??
        throw new InvalidOperationException("Null AppConfig");

    private IModLibraryService _Lib => ServiceLocator.Resolve<IModLibraryService>() ??
        throw new InvalidOperationException("Null Library/Null Library root path");

    public string AbsPath => Path.Combine(_Lib.ModsPath, PathWithinLibraryMods);

    /// <summary>
    /// Where, relative to the library root, this entry's files are stored.'
    /// </summary>
    public string PathWithinLibraryMods => Path.Combine(
        ModConcept.EntriesSubpath,
        ModMetadata.Version.ToString(),
        ModMetadata.VariantString);

    public string HumanReadableIdentifier => ModMetadata.Name + " " + ModMetadata.Version;

    public ModEntrySlug Slug => new(ModConcept.Slug, ModMetadata.Version);

    #endregion

    #region Constructors

    public ModEntry() { }

    public ModEntry(Mod mod, ModMetadata? modMetadata)
    {
        // RootPath = rootPath;
        ModConcept = mod;
        ModMetadata = modMetadata ?? mod.MetadataTemplate;
    }

    public ModEntry(Mod mod, Version entryVersion)
    {
        // RootPath = rootPath;
        ModConcept = mod;
        ModMetadata = mod.MetadataTemplate with { Version = entryVersion };
    }
    
    #endregion
    
    #region Factory Methods

    public static ModEntry CreateNewUnique(Mod mod, ModMetadata? modMetadata = null)
    {
        return new ModEntry(mod, modMetadata);
    }
    
    #endregion
    
    #region Methods

    public bool ExistsOnDisk() 
    {
        return Directory.Exists(ModConcept.EntriesSubpath);
    }
    
    #endregion
}