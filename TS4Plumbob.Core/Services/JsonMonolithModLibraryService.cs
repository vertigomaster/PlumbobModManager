using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private Dictionary<Guid, ModEntry> _runtimeModsByGuid;

    public IReadOnlyList<ModEntry> ModList => _modList;
    public IReadOnlyDictionary<Guid, ModEntry> ModsByGuid => _runtimeModsByGuid;

    public JsonMonolithModLibraryService()
    {
        _runtimeModsByGuid = [];
        _modList = [];
    }
    
    public static JsonMonolithModLibraryService? LoadFromFile(string modLibraryFile)
    {
        string modLibraryString = File.ReadAllText(modLibraryFile);
        var lib = JsonSerializer.Deserialize<JsonMonolithModLibraryService>(modLibraryString);
        lib?.InitializeFromSerializedData();
        return lib;
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
    
    public void SaveToFile(string modLibraryFile) => File.WriteAllText(modLibraryFile, Serialize());

    public string Serialize() => JsonSerializer.Serialize(this);

    public ModEntry? GetMod(Guid id) => _runtimeModsByGuid.GetValueOrDefault(id);

    public void AddMod(ModEntry modEntry)
    {
        if(!_distinctModLut.Add(modEntry)) return;
        _modList.Add(modEntry);
        _runtimeModsByGuid.Add(modEntry.Id, modEntry);
    }


    public void RemoveMod(ModEntry modEntry)
    {
        throw new NotImplementedException("need to set up a way to remove mods from rigs");
        
        //TODO: trigger event that can inform all rigs, prob via a service
        if(!_distinctModLut.Remove(modEntry)) return;
        _modList.Remove(modEntry);
        _runtimeModsByGuid.Remove(modEntry.Id);
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

    private void InitializeFromSerializedData()
    {
        _distinctModLut = _modList.ToHashSet();
        _runtimeModsByGuid = _modList.ToDictionary(mod => mod.Id);
    }

}

