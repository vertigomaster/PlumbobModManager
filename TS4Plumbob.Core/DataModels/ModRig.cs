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
    private HashSet<ModEntry> _modLut;

    private List<ModEntry> _orderedInstallList;
    public IReadOnlyList<ModEntry> OrderedInstallList => _orderedInstallList;

    public int Count => _modLut.Count;

    #region Constructors/Factories

    public ModRig()
    {
        _orderedInstallList = [];
        _modLut = [];
    }

    public ModRig(ModRigSnapshot snapshot)
    {
        _orderedInstallList = snapshot.OrderedInstallList
            .Where(m => m != null)
            .ToList()!;
        _modLut = new(_orderedInstallList);
    }
    
    public ModRig(IEnumerable<ModEntry> orderedInstallList)
    {
        _orderedInstallList = orderedInstallList.ToList();

        AssertNoDuplicateMods();
        
        _modLut = new(_orderedInstallList);
    }

    public static ModRig? FromSerializedData(string serializedData)
    {
        return JsonSerializer.Deserialize<ModRig>(serializedData, AppConfig.LibrarySerializerOptions);
    }

    #endregion

    #region Validation Checks

    private void AssertNoDuplicateMods()
    {
        if (_orderedInstallList.Distinct().Count() != _orderedInstallList.Count)
            throw new ArgumentException("OrderedInstallList must have no duplicate elements!");
    }

    #endregion

    #region Queries

    public IEnumerable<ModEntry> GetAllMods()
    {
        return _orderedInstallList;
    }
    
    public ModEntry? GetMod(ModEntry mod)
    {
        return _orderedInstallList.FirstOrDefault(m => m == mod);
    }
    
    public bool Contains(ModEntry mod)
    {
        return _modLut.Contains(mod);
    }

    public int GetIndexOf(ModEntry mod)
    {
        return _orderedInstallList.IndexOf(mod);
    }
    
    #endregion

    #region Mod Manipulation
    
    private bool Internal_TryAddMod(ModEntry mod)
    {
        if (!ServiceLocator.Resolve<IModLibraryService>().IsValidMod(mod)) return false;

        return _modLut.Add(mod);
    }
    
    private bool Internal_TryRemoveMod(ModEntry mod)
    {
        return _modLut.Remove(mod);
    }
    
    public bool TryAddModToEnd(ModEntry mod)
    {
        if(!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Add(mod);
        return true;
    }

    public bool TryAddModToStart(ModEntry mod)
    {
        if (!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(0, mod);
        return true;
    }

    public bool TryAddModToIndex(ModEntry mod, int index)
    {
        if (!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(index, mod);
        return true;
    }

    public bool TryMoveModToIndex(ModEntry mod, int index)
    {
        if (!_modLut.Contains(mod)) return false;
        _orderedInstallList.Remove(mod);
        _orderedInstallList.Insert(index, mod);
        return true;
    }

    public bool TryRemoveMod(ModEntry mod)
    {
        if (!Internal_TryRemoveMod(mod)) return false;
        _orderedInstallList.Remove(mod);
        return true;
    }

    #endregion
}