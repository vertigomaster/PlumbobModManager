using System.Text.Json;
using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// Represents a set of mods that would be found in a rig.
/// Denotes a runtime-only hash for lookups, and a serialized ordered install List
/// </summary>
[Serializable]
public class ModRig
{
    /// <summary>
    /// hash of the specific mod entries
    /// </summary>
    private HashSet<ModEntry> _modEntryLut;
    /// <summary>
    /// Hash of the represented mods.
    /// Used to avoid adding multiple entries for the same mod.
    /// </summary>
    private HashSet<Mod> _modLut;

    private List<ModEntry> _orderedInstallList;
    
    public IReadOnlyList<ModEntry> OrderedInstallList => _orderedInstallList;
    public int Count => _modEntryLut.Count;

    #region Constructors/Factories

    public ModRig()
    {
        _orderedInstallList = [];
        _modEntryLut = [];
        _modLut = [];
    }

    public ModRig(ModRigSnapshot snapshot)
    {
        _orderedInstallList = snapshot.OrderedInstallList.ToList();
        _InitModRigFromInstallOrder();
    }

    public ModRig(IEnumerable<ModEntry> orderedInstallList)
    {
        _orderedInstallList = orderedInstallList.ToList();
        _InitModRigFromInstallOrder();
    }

    private void _InitModRigFromInstallOrder()
    {
        _modEntryLut = new(_orderedInstallList);
        
        Mod[] potentialMods = _orderedInstallList
            .Select(entry => entry.ModConcept)
            .Distinct()
            .ToArray()!;
        
        if(potentialMods.Length != _modEntryLut.Count)
            throw new ArgumentException(
                "Mod count differs from mod entry count - " +
                "This implies multiple entries for the same mod, " +
                "which is not allowed.");
        
        _modLut = new(potentialMods);
    }

    public static ModRig? FromSerializedData(string serializedData)
    {
        return JsonSerializer.Deserialize<ModRig>(serializedData, AppConfig.LibrarySerializerOptions);
    }

    #endregion

    public void InitializeAfterDeserialization()
    {
        _InitModRigFromInstallOrder();
    }

    #region Validation Checks

    private void AssertNoDuplicateModEntries()
    {
        if (_orderedInstallList.Distinct().Count() != _orderedInstallList.Count)
            throw new ArgumentException("OrderedInstallList must have no duplicate mod entries elements!");
    }

    #endregion

    #region Queries

    public IEnumerable<ModEntry> GetAllMods()
    {
        return _orderedInstallList;
    }
    
    public bool Contains(ModEntry mod)
    {
        return _modEntryLut.Contains(mod);
    }

    public int GetIndexOf(ModEntry mod)
    {
        return _orderedInstallList.IndexOf(mod);
    }
    
    #endregion

    #region Mod Manipulation
    
    public bool TryAddModEntryToEnd(ModEntry mod)
    {
        if(!_TryAddMod(mod)) return false;
        _orderedInstallList.Add(mod);
        return true;
    }

    public bool TryAddModToStart(ModEntry mod)
    {
        if (!_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(0, mod);
        return true;
    }

    public bool TryAddModToIndex(ModEntry mod, int index)
    {
        if (!_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(index, mod);
        return true;
    }

    public bool TryMoveModToIndex(ModEntry mod, int index)
    {
        if (!_modEntryLut.Contains(mod)) return false;
        _orderedInstallList.Remove(mod);
        _orderedInstallList.Insert(index, mod);
        return true;
    }

    public bool TryRemoveMod(ModEntry mod)
    {
        if (!_TryRemoveMod(mod)) return false;
        _orderedInstallList.Remove(mod);
        return true;
    }


    private bool _TryAddMod(ModEntry modEntry)
    {
        if (!ServiceLocator.TryResolve(out IModLibraryService? lib))
            throw new InvalidOperationException("ModLibraryService not registered!");

        //Only add if the entry's mod is valid and if the entry isn't already in the rig
        return lib!.IsValidMod(modEntry.ModConcept) && _modEntryLut.Add(modEntry) && _modLut.Add(modEntry.ModConcept);

        //TODO: fire events or utilize an observer pattern?
    }

    private bool _TryRemoveMod(ModEntry mod)
    {
        //no extra checks since it's being removed anyway
        return _modEntryLut.Remove(mod) && _modLut.Remove(mod.ModConcept);

        //TODO: fire events or utilize an observer pattern?
    }

    #endregion
}