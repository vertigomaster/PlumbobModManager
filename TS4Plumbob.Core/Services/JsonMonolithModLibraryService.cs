using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TS4Plumbob.Core.DataModels.IdTypes;

namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// The set of all mods known to Plumbob
/// </summary>
[Serializable]
public class JsonMonolithModLibraryService : IModLibraryService
{
    //scans filesystem to find them all
    
    //stores them all
    //caches
    

    [JsonInclude]
    [JsonPropertyName("mods")]
    private List<ModEntry> _modList;

    [JsonIgnore]
    private HashSet<ModEntry> _distinctModLut;

    [JsonIgnore]
    private Dictionary<ModEntryId, ModEntry> _runtimeModsByGuid;

    public IReadOnlyList<ModEntry> ModList => _modList;
    public IReadOnlyDictionary<ModEntryId, ModEntry> ModsByGuid => _runtimeModsByGuid;

    private ModRig _activeRig;
    private List<ModRig> _rigs;

    /// <inheritdoc />
    public ModRig? ActiveRig { get; set; }

    [JsonInclude]
    [JsonPropertyName("activeProfile")]
    /// <inheritdoc />
    public List<ModRig> Rigs { get; }

    public JsonMonolithModLibraryService()
    {
        _runtimeModsByGuid = [];
        _modList = [];
        _distinctModLut = [];
        _rigs = [];
        _activeRig = null;
    }

    #region Implementation of IService

    /// <inheritdoc />
    public void OnRegister(Type type)
    {
        //is this a good idea? prob not
        // ValidateLibrary();
    }

    /// <inheritdoc />
    public void OnUnregister(Type type) { }

    #endregion

    public static JsonMonolithModLibraryService? LoadFromFile(string modLibraryFile)
    {
        Debug.WriteLine("Loading mod library from file: " + modLibraryFile + "...");
        string modLibraryString = File.ReadAllText(modLibraryFile);
        var lib = Deserialize(modLibraryString);
        
        if(lib == null)
        {
            Debug.WriteLine("Failed to deserialize mod library from file. Returned null.");
            return null;
        }
        
        lib.InitializeFromSerializedData();
        var report = lib.ValidateLibrary();
        if(report.HasErrors)
        {
            Debug.WriteLine("Mod library validation failed. Should re-scan.");
            // return null;
            //TODO: mod scan (via the crawler service?)
        }

        if (report.HasMissingMods)
        {
            Debug.WriteLine("Mod library is missing certain mods. Should re-scan.");
            //TODO: mod scan (via the crawler service?)
        }
        
        Debug.WriteLine("Mod library loaded successfully.");
        return lib;
    }

    public string Serialize() => JsonSerializer.Serialize(this);
    public static JsonMonolithModLibraryService? Deserialize(string serializedData) => JsonSerializer.Deserialize<JsonMonolithModLibraryService>(serializedData);
    
    public void SaveToFile(string modLibraryFile)
    {
        Debug.WriteLine("Saving mod library to file: " + modLibraryFile + "...");
        File.WriteAllText(modLibraryFile, Serialize());
        Debug.WriteLine("Mod library saved successfully.");
    }


    public ModEntry? GetModEntry(ModEntryId id) => _runtimeModsByGuid.GetValueOrDefault(id);

    /// <inheritdoc />
    public Mod? GetMod(ModId id)
    {
        return null;
    }

    /// <inheritdoc />
    public Mod? GetModFromEntry(ModEntryId id)
    {
        return null;
    }

    public void AddMod(ModEntry modEntry)
    {
        if(!_distinctModLut.Add(modEntry)) return;
        _modList.Add(modEntry);
        _runtimeModsByGuid.Add(modEntry.Id, modEntry);
        Debug.WriteLine($"Added mod '{modEntry.HumanReadableIdentifier}' " +
            $"with ID '{modEntry.Id}' to the library.");
        Debug.WriteLine($"Mod library now contains {_modList.Count} mods.");
    }


    public void RemoveMod(ModEntry modEntry)
    {
        if(!_distinctModLut.Remove(modEntry)) return;
        _modList.Remove(modEntry);
        _runtimeModsByGuid.Remove(modEntry.Id);
        
        Debug.WriteLine($"Removed mod '{modEntry.HumanReadableIdentifier}' " +
            $"with ID '{modEntry.Id}' from the library.");
        Debug.WriteLine($"Mod library now contains {_modList.Count} mods.");
        
        //TODO: trigger event that can inform all rigs, prob via a service?
        //as they'll need to be removed from the rig manifest as well -
        //either now or we have them update their rig manifests once they're reloaded.
        //The cache is more for record and seeing prior state than as a source of truth.
    }

    public bool IsValidMod(ModEntry? mod)
    {
        //exists, is present, and its guid matches
        return mod != null && 
            _distinctModLut.Contains(mod) && 
            _runtimeModsByGuid.GetValueOrDefault(mod.Id) == mod;
    }

    public ModLibraryValidationResult ValidateLibrary()
    {
        var results = new ModValidationResult[_modList.Count];

        for (var index = 0; index < _modList.Count; index++)
        {
            var modEntry = _modList[index];

            bool valid = IsValidMod(modEntry);
            bool exists = modEntry.ExistsOnDisk();
            results[index] = new ModValidationResult(modEntry, valid, exists);
        }

        return new ModLibraryValidationResult(results);
    }

    /// <inheritdoc />
    ModRig IModLibraryService.ActiveRig
    {
        get => _activeRig;
        set => _activeRig = value;
    }

    /// <inheritdoc />
    List<ModRig> IModLibraryService.Rigs => _rigs;

    public IEnumerable<ModRigElementMetadata> GetVisibleMods()
    {
        if (ActiveRig == null)
            return Enumerable.Empty<ModRigElementMetadata>();
        
        //visible mods are all the mods that have
        //entries present in the active rig.
        var entryIdList = ActiveRig.OrderedInstallList;
        return entryIdList.Select(entryId => {
            var entry = GetModFromEntry(entryId);
            if (entry == null)
                return null;
            
            return new ModRigElementMetadata(entry.Id);
        }).Where(m => m != null)!;
        
    }


    /// <summary>
    /// Initializes the full mod library runtime from its serialized data.
    /// </summary>
    private void InitializeFromSerializedData()
    {
        Debug.WriteLine("Initializing mod library from serialized data...");
        _distinctModLut = _modList.ToHashSet();
        _runtimeModsByGuid = _modList.ToDictionary(mod => mod.Id);
        
        // //get active profile from guid
        // if (_activeProfileId != Guid.Empty)
        // {
        //     ActiveRig = Rigs.FirstOrDefault(profile => profile.RigId == _activeProfileId);
        // }
        
        Debug.WriteLine($"Mod library initialized successfully with {_modList.Count} mods.");
    }

}

