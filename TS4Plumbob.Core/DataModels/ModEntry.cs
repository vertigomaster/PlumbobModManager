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

    //Deserializers struggle with circular dependencies (even if they are resolvable).
    //To avoid that, we purposely do NOT serialize back-references and instead resolve
    //them manually in subsequent steps.
    [JsonIgnore]
    public Mod? ModConcept { get; internal init; }

    #endregion

    #region Computed Properties

    private AppConfig _AppConfig => ServiceLocator.Resolve<AppConfig>() ??
        throw new InvalidOperationException("Null AppConfig");

    private IAsyncModLibraryService _Lib => ServiceLocator.Resolve<IAsyncModLibraryService>() ??
        throw new InvalidOperationException("Null Library/Null Library root path");

    public string AbsPath => Path.Combine(ModConcept.EntriesAbsolutePath, FolderName);
    
    public string FolderName => ModSlug.SanitizeForSlug(ModMetadata.Version.ToString()) +
        ModSlug.SanitizeForSlug(ModMetadata.VariantString);

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
        return Directory.Exists(AbsPath);
    }
    
    #endregion
    public virtual bool Equals(ModEntry? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        //ModConcept is a non-serialized circular back-reference;
        //it cannot reliably be used for equality comparison (because it cannot be reliably used for hash code generation)
        //So we do not count it, even though TECHNICALLY you could have two entries with identical metadata but different mods. 
        //Because you really shouldn't have two entries with identical metadata.
        //That's a validator task, not an equality task.
        return ModMetadata.Equals(other.ModMetadata);
    }

    public override int GetHashCode()
    {
        //ModConcept is a non-serialized circular back-reference; it cannot reliably be used for hash code generation
        // return HashCode.Combine(ModMetadata, ModConcept.Slug);
        return ModMetadata.GetHashCode();
    }
    
    public override string ToString()
    {
        return $"ModEntry '{HumanReadableIdentifier}'";
    }
}