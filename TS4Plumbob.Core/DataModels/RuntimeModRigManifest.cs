using System.Text.Json;
using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// Represents a set of mods that would be found in a rig.
/// Denotes a runtime-only hash for lookups, and a serialized ordered install List
/// </summary>
[Serializable]
public class RuntimeModRigManifest
{
    // [NonSerialized]
    // private HashSet<ModEntry> _modLut;
    [NonSerialized]
    private HashSet<Guid> _modLut;

    // [JsonInclude]
    // [JsonPropertyName("orderedInstallList")]
    private List<Guid> _orderedInstallList;
    public IReadOnlyList<Guid> OrderedInstallList => _orderedInstallList;

    public int Count => _modLut.Count;

    #region Constructors/Factories

    public RuntimeModRigManifest()
    {
        _orderedInstallList = [];
        _modLut = [];
        // _modGuidLut = [];
    }

    public RuntimeModRigManifest(ModManifestSnapshot snapshot)
    {
        //guid lookup from the pool of mods
        _orderedInstallList = snapshot.OrderedInstallList.ToList();
        _modLut = new HashSet<Guid>(_orderedInstallList);
        // _modGuidLut = new HashSet<Guid>(_modLut.Select(mod => mod.Id));
    }
    
    public RuntimeModRigManifest(IEnumerable<Guid> orderedInstallList)
    {
        _orderedInstallList = orderedInstallList.ToList();

        AssertNoDuplicateMods();
        
        InitializeFromSerializedData();
    }
    
    // public RuntimeModRigManifest(IEnumerable<Guid> orderedInstallList, HashSet<Guid> modLut)
    // {
    //     _orderedInstallList = orderedInstallList.ToList();
    //     
    //     AssertListAndHashModCountsMatch(modLut);
    //     AssertListAndHashModsMatch(modLut);
    //     AssertNoDuplicateMods();
    //     
    //     _modLut = modLut;
    //     _modGuidLut = new HashSet<Guid>(_modLut.Select(mod => mod.Id));
    // }

    public static RuntimeModRigManifest? FromSerializedData(string serializedData)
    {
        RuntimeModRigManifest? manifest = JsonSerializer.Deserialize<RuntimeModRigManifest>(serializedData);
        manifest?.InitializeFromSerializedData();
        return manifest;
    }

    public void InitializeFromSerializedData()
    {
        // Repopulate the runtime hashsets from the list on deserialization
        _modLut = new HashSet<Guid>(_orderedInstallList);

        foreach (var modId in _orderedInstallList)
        {
            var mod = ServiceLocator.Resolve<IModLibraryService>().GetMod(modId);
            if (mod == null)
            {
                //todo: some notification that this rig refers to a missing mod
            }
        }
        
        // _modGuidLut = new HashSet<Guid>(_modLut.Select(mod => mod.Id));
    }

    #endregion

    #region Validation Checks

    // private void AssertListAndHashModsMatch(HashSet<ModEntry> modLut)
    // {
    //     if (_orderedInstallList.Any(modId => !modLut.Contains(modId)))
    //         throw new ArgumentException("ModLut and OrderedInstallList must have the same elements!");
    // }
    //
    // private void AssertListAndHashModCountsMatch(HashSet<ModEntry> modLut)
    // {
    //     if (modLut.Count != _orderedInstallList.Count) 
    //         throw new ArgumentException("ModLut and OrderedInstallList must have the same number of elements!");
    // }

    private void AssertNoDuplicateMods()
    {
        if (_orderedInstallList.Distinct().Count() != _orderedInstallList.Count)
            throw new ArgumentException("OrderedInstallList must have no duplicate elements!");
    }

    #endregion

    #region Queries

    public ModEntry? GetMod(Guid modId)
    {
        if(!Contains(modId)) return null;
        var mod = ServiceLocator.Resolve<IModLibraryService>().GetMod(modId);
        return mod;
    }
    
    public bool Contains(Guid modId) => 
        _modLut.Contains(modId);
    
    public bool Contains(ModEntry mod)
    {
        // var mod = ServiceLocator.Resolve<ModLibraryService>().GetMod(modId);
        return _modLut.Contains(mod.Id);
    }

    public int GetIndexOf(Guid modId)
    {
        if (!_modLut.Contains(modId)) return -1;

        // for (int i = 0; i < _orderedInstallList.Count; i++)
        //     if (_orderedInstallList[i].Id == modId) return i;
        
        return _orderedInstallList.IndexOf(modId);
    }
    
    #endregion

    #region Mod Manipulation
    
    public bool TryAddModToEnd(Guid modId)
    {
        if(!Internal_TryAddMod(modId)) return false;
        _orderedInstallList.Add(modId);
        return true;
    }

    private bool Internal_TryAddMod(ModEntry mod)
    {
        //mods must first be added to the library. thats a litle annoying but makes this stuff a lot easier to maintain.
        if (!ServiceLocator.Resolve<IModLibraryService>().IsValidMod(mod)) return false;

        return _modLut.Add(mod.Id);
    }
    
    private bool Internal_TryRemoveMod(ModEntry mod)
    {
        //mods must first be added to the library. thats a litle annoying but makes this stuff a lot easier to maintain.
        // if (!ServiceLocator.Resolve<ModLibraryService>().IsValidMod(mod)) return false;

        return _modLut.Remove(mod.Id);
    }
    
    private bool Internal_TryRemoveMod(Guid modId)
    {
        //mods must first be added to the library. thats a litle annoying but makes this stuff a lot easier to maintain.
        // if (!ServiceLocator.Resolve<ModLibraryService>().IsValidMod(mod)) return false;

        return _modLut.Remove(modId);
    }
    
    private bool Internal_TryAddMod(Guid modId)
    {
        //mods must first be added to the library. thats a litle annoying but makes this stuff a lot easier to maintain.
        if (ServiceLocator.Resolve<IModLibraryService>().GetMod(modId) == null) return false;

        return _modLut.Add(modId);
    }
    
    public bool TryAddModToEnd(ModEntry mod)
    {
        if(!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Add(mod.Id);
        return true;
    }

    public bool TryAddModToStart(ModEntry mod)
    {
        if (!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(0, mod.Id);
        return true;
    }

    public bool TryAddModToIndex(ModEntry mod, int index)
    {
        if (!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Insert(index, mod.Id);
        return true;
    }

    public bool TryMoveModToIndex(ModEntry mod, int index)
    {
        if (!Internal_TryAddMod(mod)) return false;
        _orderedInstallList.Remove(mod.Id);
        _orderedInstallList.Insert(index, mod.Id);
        return true;
    }

    public bool TryRemoveMod(ModEntry mod)
    {
        if (!Internal_TryRemoveMod(mod)) return false;
        _orderedInstallList.Remove(mod.Id);
        return true;
    }

    #endregion
}