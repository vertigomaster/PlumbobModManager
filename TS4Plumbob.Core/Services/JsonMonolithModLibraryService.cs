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
    internal List<Mod> _serializableModList;

    [JsonIgnore]
    private HashSet<Mod> _distinctModLut;

    [JsonIgnore]
    public IReadOnlyList<Mod> ModList => _serializableModList;

    [JsonInclude]
    [JsonPropertyName("activeRig")]
    internal ModRig? _activeRig;

    [JsonInclude]
    [JsonPropertyName("rigs")]
    internal List<ModRig> _rigs;
    /// <inheritdoc />
    
    [JsonIgnore]
    public ModRig? ActiveRig
    {
        get => _activeRig;
        internal set => _activeRig = value;
    }

    [JsonIgnore]
    /// <inheritdoc />
    public IReadOnlyList<ModRig> Rigs => _rigs;

    public JsonMonolithModLibraryService()
    {
        _serializableModList = [];
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

        CreateDefaultRigIfNone();
    }

    /// <inheritdoc />
    public void OnUnregister(Type type) { }

    #endregion

    internal static string ModLibraryConfigFile
    {
        get
        {
            var appConfig = ServiceLocator.Resolve<AppConfig>();
            return Path.Combine(appConfig?.UserSettings.ModLibraryPath, "modlibrary.json");
        }
    }

    public static async Task<JsonMonolithModLibraryService?> LoadFromFileAsync()
    {
        string libConfigFile = ModLibraryConfigFile;
        
        Debug.WriteLine("Loading mod library from file: " + libConfigFile + "...");
        if (!File.Exists(libConfigFile))
        {
            Debug.WriteLine("Mod library file does not exist: " + libConfigFile);
            return null;
        }
        
        string modLibraryString = await File.ReadAllTextAsync(libConfigFile);
        
        //while the deserializer does have an async overload,
        //it would require refactoring stuff into a stream, which
        //sounds like a lot of work that may not be worth it, so we're just doing this.
        var lib = await DeserializeAsync(modLibraryString);
        
        if(lib == null)
        {
            Debug.WriteLine("Failed to deserialize mod library from file. Returned null.");
            return null;
        }
        
        lib.InitializeFromSerializedData();
        
        //this one can take a while, so we'll do it in a background thread.
        var report = await Task.Run(lib.ValidateLibrary);
        
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

    public async Task<string> SerializeAsync() => await Task.Run(() => JsonSerializer.Serialize(this, AppConfig.LibrarySerializerOptions));

    /// <summary>
    /// Deserializes the given string into a mod library.
    /// </summary>
    /// <param name="serializedData"></param>
    /// <returns></returns>
    /// <remarks>Currently does an awkward Task.Run() wrap,
    /// but correctly implementing JsonSerializer.DeserializeAsync()
    /// would require a bit of refactoring and other work that may
    /// not be worth the effort (yet). </remarks>
    public static async Task<JsonMonolithModLibraryService?> DeserializeAsync(
        string serializedData) => await Task.Run(() =>
            JsonSerializer.Deserialize<JsonMonolithModLibraryService>(
                serializedData, AppConfig.LibrarySerializerOptions));

    //TODO: async?
    public async Task SaveToFileAsync(string? overridePath=null)
    {
        string modLibraryFile = overridePath ?? ModLibraryConfigFile;
        
        Debug.WriteLine("Saving mod library to file: " + modLibraryFile + "...");
        
        string? modLibraryDir = Path.GetDirectoryName(modLibraryFile);
        if (modLibraryDir == null)
        {
            Debug.WriteLine("Mod library directory is null, cannot save file.");
            return;
        }
        Directory.CreateDirectory(modLibraryDir);
        await File.WriteAllTextAsync(modLibraryFile, await SerializeAsync());
        
        Debug.WriteLine("Mod library saved successfully.");
    }

    /// <inheritdoc />
    // public async Task CopyDirectorySubtreeIntoModEntryDirAsync(Uri subtreeUri, ModEntry thisEntry)
    // {
    //     
    //     
    //     await CopyDirectorySubtreeIntoModEntryAsync(subtreeUri.AbsolutePath, thisEntry);
    // }

    public async Task CopyFolderIntoModEntryAsync(string subtreePath, ModEntry thisEntry)
    {
        await CopyDirectorySubtreeIntoModEntryAsync(subtreePath, thisEntry.AbsPath);
    }

    public async Task CopyDirectorySubtreeIntoModEntryAsync(string subtree, string modEntryPath)
    {
        Directory.CreateDirectory(modEntryPath);
        
        //enumerate all files (better than getting all up front)
        var subtreeFilepaths = Directory.EnumerateFiles(
            subtree, "*", SearchOption.AllDirectories);

        //set it up to split up the work among all available cores
        // var options = ;
        
        //instead of the "raw" task library, utilizes a parallelized "job-style" approach to run the foreach with parallel support.
        await Parallel.ForEachAsync(subtreeFilepaths, 
            new ParallelOptions {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            async (absSrcFilePath, cancelToken) => 
            {
                string relFilePath = Path.GetRelativePath(
                    subtree, absSrcFilePath);
                string destPath = Path.Combine(
                    modEntryPath, relFilePath);
                
                //ensure destination directory actually exists
                //(bc that's not actually guaranteed unfortunately)
                string? destDir = Path.GetDirectoryName(destPath);
                if (destDir != null) Directory.CreateDirectory(destDir);
                //if no valid dir, I guess there's nothing to create?

                //Free up the parallel threads by using the
                //well-optimized worker thread pool to handle the work in the background. 
                await Task.Run(
                    () => File.Copy(absSrcFilePath, destPath, overwrite: true), 
                    cancelToken);
            }
        );
    }


    // public ModEntry? MaybeGetModEntry(ModEntrySlug humanReadableIdentifier)

    // {

    //     return _distinctModLut.FirstOrDefault(m => m.HumanReadableIdentifier == humanReadableIdentifier);

    // }


    // public Mod? MaybeGetMod(ModEntrySlug modEntrySlug)

    // {

    //     return _distinctModLut.FirstOrDefault(m => m.HumanReadableIdentifier == modEntrySlug)?.ModConcept;

    // }


    // public Mod? MaybeGetMod(ModSlug modSlug)

    // {

    //     return _distinctModLut.FirstOrDefault(entry => entry.ModConcept.Slug == modSlug)?.ModConcept;

    // }


    public bool TryAddMod(Mod mod, bool trySilently = false)
    {
        //attempts to add the mod
        if(!_distinctModLut.Add(mod))
        {
            if(!trySilently) Console.WriteLine(
                $"Failed to add mod '{mod.Name}' ('{mod.Slug}') to the library. Duplicate entry detected.");
            
            return false;
        }
        
        _serializableModList.Add(mod);

        Debug.WriteLine($"Add mod '{mod.Name}' to the library.");
        Debug.WriteLine($"Mod library now contains {_serializableModList.Count} mods.");

        return true;
    }

    //just realized the use case for this makes no sense; mod entries are always created with respect to an EXISTING mod. There's no reason or way to add a mod entry without adding its mod, at which point you should just add the mod directly.

    //Will remove the commented code soon.

    // public bool TryAddModEntry(ModEntry modEntry)

    // {

    //     TryAddMod(modEntry.ModConcept, trySilently:true); //ensures the entry's mod is added to the library.

    //     

    //     

    //     

    //     //make all the checks before actually mutating state

    //     if(_distinctModLut.Contains(modEntry))

    //     {

    //         Console.WriteLine(

    //             $"Failed to add mod '{modEntry.HumanReadableIdentifier}' to the library. Duplicate entry detected.");

    //         return false;

    //     }

    //

    //     //mutate state

    //     _distinctModLut.Add(modEntry);

    //     _serializableModList.Add(modEntry);

    //     

    //     Debug.WriteLine($"Added mod '{modEntry.HumanReadableIdentifier}' to the library.");

    //     Debug.WriteLine($"Mod library now contains {_serializableModList.Count} mods.");

    //     return true;

    // }


    public bool TryRemoveMod(Mod mod, bool trySilently = false)
    {
        if (!_distinctModLut.Remove(mod))
        {
            if (!trySilently) Console.WriteLine($"Failed to remove mod '{mod.Name}' from the library. Mod not found.");
            return false;
        }
        _serializableModList.Remove(mod);
        
        Debug.WriteLine($"Removed mod '{mod.Name}' from the library.");
        Debug.WriteLine($"Mod library now contains {_serializableModList.Count} mods.");
        
        //TODO: trigger event that can inform all rigs, prob via a service?
        //as they'll need to be removed from the rig manifest as well -
        //either now or we have them update their rig manifests once they're reloaded.
        //The cache is more for record and seeing prior state than as a source of truth.
        
        return true;
    }

    /// <summary>
    /// Determines if the given mod is valid for use/etc.
    /// This method is mostly just a sanity check, though its vagueness makes it less useful...
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    /// <remarks>
    /// This may eventually be deprecated if we never have other validation checks.
    /// </remarks>
    public bool IsValidMod(Mod? mod)
    {
        //exists and is present
        return mod != null;
        
        /*
         Keeping this comment as a warning in case you forget and come back here:
         
         Containment in the LUT cannot serve as validation criteria!
        
         had to remove due to cyclical issues; only valid mods should be added,
         which means some valid mods won't yet be in the lookup table, therefore,
         the LUT containing them cannot serve as a valid validity check 
        */
        // _distinctModLut.Contains(mod);
    }

    public ModLibraryValidationResult ValidateLibrary()
    {
        var results = new List<ModValidationResult>(_serializableModList.Count);

        foreach(var mod in _serializableModList)
        {
            bool valid = IsValidMod(mod);
            foreach (var entry in mod.Entries)
            {
                bool exists = entry.ExistsOnDisk();
                results.Add(new ModValidationResult(entry, valid, exists));
            }
        }

        return new ModLibraryValidationResult(results.ToArray());
    }

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
        _distinctModLut = _serializableModList.ToHashSet(); 
        
        Debug.WriteLine($"Mod library initialized successfully with {_serializableModList.Count} mods.");
    }

    private void CreateDefaultRigIfNone()
    {
        if (_activeRig == null)
        {
            if (_rigs.Count > 0)
            {
                _activeRig = _rigs[0];
            } 
            else
            {
                _activeRig = new ModRig();
                _rigs.Add(_activeRig);
            }
        }
    }
}

