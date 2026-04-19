using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

public interface IModLibraryService : IService
{
    Task<string> SerializeAsync();
    
    //not used - bring back in some form if needed later.
    // ModEntry? MaybeGetModEntry(ModEntrySlug modEntryModEntrySlug);
    // Mod? MaybeGetMod(ModEntrySlug modEntrySlug);
    bool TryAddMod(Mod mod, bool trySilently = false);
    bool TryRemoveMod(Mod mod, bool trySilently = false);
    bool IsValidMod(Mod? mod);
    ModLibraryValidationResult ValidateLibrary();
    
    ModRig? ActiveRig { get; }
    
    AppConfig Config => ServiceLocator.Resolve<AppConfig>() ??
        throw new InvalidOperationException("Null AppConfig");
    
    /// <summary>
    /// The root path of the library. Not necessarily the root of all mod folders.
    /// There's other data that makes up a library, like rigs, author profiles, and configs.
    /// For the root where all mod folders are located under, see <see cref="ModsPath"/>.    
    /// </summary>
    /// <example>
    /// <code>"D:\Modding\The Sims 4\PlumbobMM"</code>
    /// </example>
    string RootPath => Config.UserSettings.ModLibraryPath;
    
    //TODO: move all keyword names to a central source - constants for now, but possibly configurable later.
    /// <summary>
    /// The path to the library's mods folder. All mods are stored here.
    /// </summary>
    /// <example>
    /// <code>"D:\Modding\The Sims 4\PlumbobMM\mods"</code>
    /// </example>
    string ModsPath => Path.Combine(RootPath, "mods");
    
    /// <summary>
    /// The list of all profiles in the library. 
    /// </summary>
    IReadOnlyList<ModRig> Rigs { get; }

    IEnumerable<ModEntry> GetVisibleMods();
    Task SaveToFileAsync(string? overridePath=null);
    // void CopyDirectorySubtreeIntoModEntryDir(Uri subtreeUri, ModEntry thisEntry);
    // Task CopyDirectorySubtreeIntoModEntryDirAsync(Uri subtreeUri, ModEntry thisEntry);
    // void CopyDirectorySubtreeIntoModEntry(string subtreePath, ModEntry thisEntry);
    Task CopyFolderIntoModEntryAsync(string subtreePath, ModEntry thisEntry);
}