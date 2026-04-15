using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

public interface IModLibraryService : IService
{
    string Serialize();
    ModEntry? GetModEntry(ModEntrySlug modEntryModEntrySlug);
    
    Mod? GetMod(ModEntrySlug modEntryModEntrySlug);
    bool TryAddMod(ModEntry modEntry);
    void RemoveMod(ModEntry modEntry);
    bool IsValidMod(ModEntry? mod);
    ModLibraryValidationResult ValidateLibrary();
    
    ModRig ActiveRig { get; set; }
    
    /// <summary>
    /// The list of all profiles in the library. 
    /// </summary>
    List<ModRig> Rigs { get; }

    IEnumerable<ModEntry> GetVisibleMods();
    void SaveToFile(string? overridePath=null);
}