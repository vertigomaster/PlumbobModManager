using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels.IdTypes;

namespace TS4Plumbob.Core.DataModels;

public interface IModLibraryService : IService
{
    string Serialize();
    ModEntry? GetModEntry(ModEntryId id);
    Mod? GetMod(ModId id);
    Mod? GetModFromEntry(ModEntryId id);
    void AddMod(ModEntry modEntry);
    void RemoveMod(ModEntry modEntry);
    bool IsValidMod(ModEntry? mod);
    ModLibraryValidationResult ValidateLibrary();
    
    ModRig ActiveRig { get; set; }
    
    /// <summary>
    /// The list of all profiles in the library. 
    /// </summary>
    List<ModRig> Rigs { get; }

    IEnumerable<ModRigElementMetadata> GetVisibleMods();
}

//
// public enum ElementStateWithinProfile
// {
//     Enabled,
//     Disabled
// }

public record ModRigElementMetadata(ModId ModId);

// [Obsolete("Use RuntimeModRigManifest instead")]
// public class ModRig(RigId rigId, string _name = "Default")
// { 
//     public RigId RigId { get; } = rigId;
//     public string Name { get; set; } = _name;
//     
//     /// <summary>
//     /// contains the ordered list of mods mentioned in the profile - be they active or not. Inactive ones are preserved in order to preserve their install order and other metadata (since they can be easily toggled)
//     /// </summary>
//     public List<ModRigElementMetadata> OrderedElements { get; set; } = new();
//
//     /// <summary>
//     /// Gets all mods in the profile as actual <see cref="ModEntry"/> instances instead of just the thin <see cref="ModRigElementMetadata"/>.
//     /// </summary>
//     /// <returns></returns>
//     public IEnumerable<ModEntry> GetAllMods()
//     {
//         var lib = ServiceLocator.Resolve<IModLibraryService>();
//         
//         //TODO: CACHE this - it will get large
//         
//         //there aren't any actual nullability issues here;
//         //it's just that the compiler can't figure it out from within
//         //the linq expression/lambdas.
//         return OrderedElements.Select(element => lib.GetMod(element.ModId))
//             .Where(mod => mod != null)!; 
//     }
// }