using System.Text.Json.Serialization;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;

namespace TS4Plumbob.Core.DataModels;

public class Mod
{
    #region Constructors
    
    //Default constructor for deserialization.
    public Mod() { }
    
    public Mod(ModMetadata? data=null) {
        MetadataTemplate = data ?? ModMetadata.Unknown;
        Slug = new ModSlug(MetadataTemplate.Name);
    }
    
    //Constructor for new mods.

    public Mod(ModSlug slug, Version? defaultVersion=null, AuthorProfile? author=null)
    {
        Slug = slug;
        MetadataTemplate = new ModMetadata(
            slug.MainId, 
            defaultVersion ?? Version.Parse("0.0.0"),
            author ?? AuthorProfile.Unknown,
            DateTime.Now);
    }

    public Mod(ModSlug slug, AuthorProfile? author = null) : this(slug, Version.Parse("0.0.0"), author) { }

    #endregion
    #region Core Properties
    
    [JsonInclude]
    [JsonPropertyName("slug")]
    [JsonPropertyOrder(100)]
    public ModSlug Slug { get; init; }

    [JsonInclude]
    [JsonPropertyName("metadataTemplate")]
    [JsonPropertyOrder(200)]
    public ModMetadata MetadataTemplate { get; init; }

    [JsonInclude]
    [JsonPropertyName("entries")]
    [JsonPropertyOrder(300)]
    public HashSet<ModEntry> Entries { get; init; } = [];
    
    #endregion
    #region Service Accessors
    
    private AppConfig _AppConfig => ServiceLocator.Resolve<AppConfig>() 
        ?? throw new InvalidOperationException(
            "Tried to Resolve AppConfig before core could bind it.");
    private IAsyncModLibraryService _Lib => ServiceLocator.Resolve<IAsyncModLibraryService>() 
        ?? throw new InvalidOperationException(
            "Tried to Resolve IAsyncModLibraryService before core could bind it.");
    
    #endregion
    #region Computed Properties
    
    /// <summary>
    /// Where, relative to the mods folder, this mod's entries are stored.
    /// </summary>
    /// <example>
    /// <c>"example_mod_slug/entries"</c>
    /// </example>
    [JsonIgnore]
    public string EntriesSubpath => "entries";
    
    /// <summary>
    /// The absolute path to the entries folder.
    /// </summary>
    /// <exception cref="InvalidOperationException">The library and the mods root path must be defined for this to ever be possible or reliable
    /// </exception>
    /// <example>
    /// Mileage may vary based on OS and depending on how up to date docs are relative to the current source code, but on Windows it could be like:
    /// <code>"D:\Modding\The Sims 4\PlumbobMM\mods\example_mod_slug\entries"</code>
    /// or on Linux:
    /// <code>"/home/user/Documents/The Sims 4/PlumbobMM/mods/example_mod_slug/entries"</code>
    /// <code>"/home/user/.plumbobmm/mods/example_mod_slug/entries"</code>
    /// Or on Mac:
    /// <code>"/Users/user/Documents/The Sims 4/PlumbobMM/mods/example_mod_slug/entries"</code>
    /// </example>
    [JsonIgnore]
    public string EntriesAbsolutePath => Path.Combine(AbsolutePath, EntriesSubpath);

    [JsonIgnore]
    public string AbsolutePath => Path.Combine(_Lib?.AbsolutePathModsFolder ??
        throw new InvalidOperationException(
            "Null Library/Null Library mods path"), Slug.ToString());
    
    [JsonIgnore]
    public string Name => MetadataTemplate.Name;
    
    #endregion
    #region Entry Creation Methods
    
    /// <summary>
    /// Easy helper to create a new entry for a mod.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="variantString"></param>
    /// <returns></returns>
    public ModEntry AddNewEntry(Version version, string variantString="")
    {
        var successfulEntry = AddNewEntry(MetadataTemplate with
        {
            Version = version
        });
        
        Console.WriteLine($"Created new entry for {successfulEntry.Slug}");
        Directory.CreateDirectory(successfulEntry.AbsPath);
        Console.WriteLine($"Created directory for {successfulEntry.Slug} at '{successfulEntry.AbsPath}'");
        
        return successfulEntry;
    }
    
    
    /// <summary>
    /// Attempts to create a new entry for a mod using the default metadata.
    /// Useful for creating the first entry more easily.
    /// </summary>
    /// <returns></returns>
    public ModEntry AddDefaultEntry() => AddNewEntry(MetadataTemplate);

    /// <summary>
    /// Easy helper to create a new entry for a mod.
    /// </summary>
    /// <param name="entryPath"></param>
    /// <param name="newMetadata"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a matching entry already exists in this mod.
    /// This is meant to be caught and bubbled up to the UI for the user.
    /// If you want to avoid this, first check with use <see cref="ContainsEntry(string, Version)"/> instead.
    /// </exception>
    public ModEntry AddNewEntry(ModMetadata newMetadata)
    {
        var newEntry = _AddNewEntryToMod_Internal(newMetadata);

        if (_AppConfig.UserSettings.AutoAddNewModsToActiveRig)
        {
            if(_Lib.ActiveRig == null)
            {
                PlumbobMsg.WriteDebugWarning(
                    "Tried to add new mod entry to active rig but no rig is active.");
            }
            else
            {
                _Lib.ActiveRig.TryAddModEntryToEnd(newEntry);
            }
        }

        return newEntry;
    }
    
    /// <summary>
    /// The raw internal method for creating a new entry. Does not account for behaviours associated with that addition - it only concerns the core internal operation.
    /// </summary>
    /// <param name="newMetadata"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private ModEntry _AddNewEntryToMod_Internal(ModMetadata newMetadata)
    {
        ModEntry newEntry = new(this, newMetadata);

        //TODO: wrap in a NonFatalPlumbobException that can be caught and handled by the front-end.
        if (!Entries.Add(newEntry))
            throw new InvalidOperationException(
                $"Entry '{newEntry.Slug}' already registered with mod {Name} ({Slug}).");

        return newEntry;
    }
    
    #endregion
    #region Query Methods
    
    public bool ContainsEntry(Version version)
    {
        return Entries.Any(e => e.ModMetadata.Version == version);
    }
    
    #endregion

    #region Overrides of Object

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if(obj is not Mod other) return false;
        
        if (!MetadataTemplate.Equals(other.MetadataTemplate)) return false;
        if (!Slug.Equals(other.Slug)) return false;
        if (Entries.Count != other.Entries.Count) return false;
        
        return Entries.SetEquals(other.Entries);
    }
    
    public override string ToString()
    {
        return $"{Name} ({Slug}) - {Entries.Count} {(Entries.Count == 1 ? "Entry" : "Entries")}";
    }

    #region Equality members

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MetadataTemplate);
        hash.Add(Slug);
        foreach (var entry in Entries)
        {
            hash.Add(entry);
        }
        return hash.ToHashCode();
    }

    #endregion

    #endregion
}