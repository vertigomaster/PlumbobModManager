using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using IDEK.Tools.ShocktroopUtils.Services;

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

    public IReadOnlyList<ModEntry> ModList => _modList;

    private ModRig? _activeRig;
    private List<ModRig> _rigs;

    /// <inheritdoc />
    [JsonInclude]
    [JsonPropertyName("activeRig")]
    public ModRig? ActiveRig { get; set; }

    [JsonInclude]
    [JsonPropertyName("rigs")]
    /// <inheritdoc />
    public List<ModRig> Rigs { get; }

    public JsonMonolithModLibraryService()
    {
        _modList = [];
        _distinctModLut = [];
        _rigs = [];
        _activeRig = null;
        Rigs = _rigs;
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

    private static string ModLibraryConfigFile
    {
        get
        {
            var appConfig = ServiceLocator.Resolve<AppConfig>();
            return appConfig.UserSettings.ModLibraryPath;
        }
    }

    public static JsonMonolithModLibraryService? LoadFromFile()
    {
        string libConfigFile = ModLibraryConfigFile;
        
        Debug.WriteLine("Loading mod library from file: " + libConfigFile + "...");
        if (!File.Exists(libConfigFile))
        {
            Debug.WriteLine("Mod library file does not exist: " + libConfigFile);
            return null;
        }
        
        string modLibraryString = File.ReadAllText(libConfigFile);
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

    public string Serialize() => JsonSerializer.Serialize(this, AppConfig.LibrarySerializerOptions);
    public static JsonMonolithModLibraryService? Deserialize(string serializedData) => JsonSerializer.Deserialize<JsonMonolithModLibraryService>(serializedData, AppConfig.LibrarySerializerOptions);
    
    public void SaveToFile()
    {
        string modLibraryFile = ModLibraryConfigFile;
        Debug.WriteLine("Saving mod library to file: " + modLibraryFile + "...");
        File.WriteAllText(modLibraryFile, Serialize());
        Debug.WriteLine("Mod library saved successfully.");
    }


    public ModEntry? GetModEntry(ModEntrySlug humanReadableIdentifier)
    {
        return _distinctModLut.FirstOrDefault(m => m.HumanReadableIdentifier == humanReadableIdentifier);
    }
    
    public Mod? GetMod(ModEntrySlug modEntryModEntrySlug)
    {
        return _distinctModLut.FirstOrDefault(m => m.HumanReadableIdentifier == modEntryModEntrySlug).ModConcept;
    }
    
    public bool TryAddMod(ModEntry modEntry)
    {
        //make all the checks before actually mutating state
        if(_distinctModLut.Contains(modEntry))
        {
            Console.WriteLine(
                $"Failed to add mod '{modEntry.HumanReadableIdentifier}' to the library. Duplicate entry detected.");
            return false;
        }

        //mutate state
        _distinctModLut.Add(modEntry);
        _modList.Add(modEntry);
        
        Debug.WriteLine($"Added mod '{modEntry.HumanReadableIdentifier}' to the library.");
        Debug.WriteLine($"Mod library now contains {_modList.Count} mods.");
        return true;
    }


    public void RemoveMod(ModEntry modEntry)
    {
        if(!_distinctModLut.Remove(modEntry)) return;
        _modList.Remove(modEntry);
        
        Debug.WriteLine($"Removed mod '{modEntry.HumanReadableIdentifier}' from the library.");
        Debug.WriteLine($"Mod library now contains {_modList.Count} mods.");
        
        //TODO: trigger event that can inform all rigs, prob via a service?
        //as they'll need to be removed from the rig manifest as well -
        //either now or we have them update their rig manifests once they're reloaded.
        //The cache is more for record and seeing prior state than as a source of truth.
    }

    public bool IsValidMod(ModEntry? mod)
    {
        //exists and is present
        return mod != null && 
            _distinctModLut.Contains(mod);
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
        get => _activeRig!;
        set => _activeRig = value;
    }

    /// <inheritdoc />
    List<ModRig> IModLibraryService.Rigs => _rigs;

    public IEnumerable<ModEntry> GetVisibleMods()
    {
        if (ActiveRig == null)
            return Enumerable.Empty<ModEntry>();
        
        //visible mods are all the mods that have
        //entries present in the active rig.
        return ActiveRig.OrderedInstallList;
    }


    /// <summary>
    /// Initializes the full mod library runtime from its serialized data.
    /// </summary>
    private void InitializeFromSerializedData()
    {
        Debug.WriteLine("Initializing mod library from serialized data...");
        _distinctModLut = _modList.ToHashSet();
        
        Debug.WriteLine($"Mod library initialized successfully with {_modList.Count} mods.");
    }

}

