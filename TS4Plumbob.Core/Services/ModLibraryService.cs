using System.Text.Json;
using System.Text.Json.Serialization;

namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// The set of all mods known to Plumbob
/// </summary>
[Serializable]
public class ModLibraryService
{
    //scans filesystem to find them all
    
    //stores them all
    //caches
    [JsonIgnore]
    private Dictionary<Guid, ModEntry> _runtimModLut;

    [JsonInclude]
    [JsonPropertyName("mods")]
    private List<ModEntry> _serializedModList;
    private HashSet<ModEntry> _distinctModLut;
    
    public IReadOnlyDictionary<Guid, ModEntry> ModLut => _runtimModLut;
    public IReadOnlyList<ModEntry> SerializedModList => _serializedModList;
    
    public ModLibraryService()
    {
        _runtimModLut = [];
        _serializedModList = [];
    }
    
    public static ModLibraryService? FromSerializedData(string modLibraryString)
    {
        var lib = JsonSerializer.Deserialize<ModLibraryService>(modLibraryString);
        lib?.InitializeFromSerializedData();
        return lib;
    }

    public string Serialize() => JsonSerializer.Serialize(this);

    public void InitializeFromSerializedData()
    {
        _runtimModLut = _serializedModList.ToDictionary(mod => mod.Id);
    }
    
    public ModEntry? GetMod(Guid id) => _runtimModLut.GetValueOrDefault(id);
    
    public void AddMod(ModEntry modEntry)
    {
        if(!_distinctModLut.Add(modEntry)) return;
        _serializedModList.Add(modEntry);
        _runtimModLut.Add(modEntry.Id, modEntry);
    }

    
    public void RemoveMod(ModEntry modEntry)
    {
        throw new NotImplementedException("need to set up a way to remove mods from rigs");
        
        //TODO: trigger event that can inform all rigs, prob via a service
        if(!_distinctModLut.Remove(modEntry)) return;
        _serializedModList.Remove(modEntry);
        _runtimModLut.Remove(modEntry.Id);
    }

    public bool IsValidMod(ModEntry? mod)
    {
        //exists, is present, and its guid matches
        return mod != null && 
            _distinctModLut.Contains(mod) && 
            _runtimModLut.GetValueOrDefault(mod.Id) == mod;
    }
}