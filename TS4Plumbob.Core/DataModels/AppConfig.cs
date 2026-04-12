using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// Serializable class for configuration "settings" that are controllable/modifiable by the "user" - hence the name.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// The root of the entire mod library. Pretty much all app-generated files are stored here.
    /// </summary>
    [JsonInclude, JsonPropertyName("modLibraryPath")]
    public string ModLibraryPath { get; set; }

    /// <summary>
    /// Folder where all the rig folders are stored. Should ideally be a subfolder of <see cref="ModLibraryPath"/>.
    /// </summary>
    /// <remarks>
    /// Rigs do not store actual source mod files, they only store rig metadata about them.
    /// </remarks>
    [JsonInclude, JsonPropertyName("profileRootPath")]
    public string RigsRootPath { get; set; }
}

/// <summary>
/// Top-level application config class. All app-wide settings and
/// configurations can be found here or on the delegated class properties. 
/// </summary>
public class AppConfig : IService
{
    public static readonly JsonSerializerOptions AppSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        AllowTrailingCommas = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    [JsonInclude, JsonPropertyName("userSettings")]
    public UserSettings UserSettings { get; } = new UserSettings();

    //TODO: contemplate whether the version should be compiled into the build for reliability

    [JsonInclude, JsonPropertyName("version")]
    public string FullVersionString { get; set; } = "0.0.0"; //TODO: include release and commit hash?

    [JsonInclude, JsonPropertyName("shortVersion")]
    public string ShortVersionString { get; set; } = "0.0.0";

    [JsonInclude, JsonPropertyName("appName")]
    public string FullAppName { get; set; } = "Plumbob Mod Manager";

    [JsonInclude, JsonPropertyName("shortAppName")]
    public string ShortAppName { get; set; } = "PlumbobMM";

    public static string AppFolder => AppContext.BaseDirectory;
    public static string AppConfigDir => Path.Combine(AppFolder, "config");
    public static string MainAppConfig => Path.Combine(AppConfigDir, "appconfig.json");

    #region Implementation of IService

    /// <inheritdoc />
    public void OnRegister(Type type) { }

    /// <inheritdoc />
    public void OnUnregister(Type type)
    {
        //do not force saving; need ability to discard changes
        //SaveToDisk();
    }

    #endregion

    public void SaveToDisk()
    {
        Debug.WriteLine("Saving AppConfig to disk...");
        string serialized = JsonSerializer.Serialize(
            this, AppSerializerOptions);
        
        string? dir = Path.GetDirectoryName(MainAppConfig);

        if (dir == null)
        {
            throw new InvalidOperationException("Configuration directory path is null. Cannot save to disk.");
        }
        
        //harmless if it already exists, creates it otherwise
        Directory.CreateDirectory(dir);
        //creates or overwrites configu file with our up to date version.
        File.WriteAllText(MainAppConfig, serialized);
        Debug.WriteLine("AppConfig saved successfully.");
    }

    public static AppConfig? LoadFromDisk()
    {
        Debug.WriteLine("Loading AppConfig from disk...");
        try
        {
            string fileText = File.ReadAllText(MainAppConfig);
            AppConfig? result = JsonSerializer.Deserialize<AppConfig>(fileText);
            if (result != null)
            {
                Debug.WriteLine("AppConfig loaded successfully.");
            }
            else
            {
                Debug.WriteLine("Warning: failed to deserialize existing AppConfig. Returning null.");
            }
            return result;
        }
        catch (DirectoryNotFoundException e)
        {
            Debug.WriteLine(
                $"Warning: AppConfig.LoadFromDisk failed - a directory was missing in " +
                $"path \"{MainAppConfig}\" : \n\t" + e.Message + " - returning null.");
            return null;
        }
        catch (FileNotFoundException e)
        {
            Debug.WriteLine(
                $"Warning: AppConfig.LoadFromDisk failed - file \"{MainAppConfig}\" not " +
                $"present: " + e.Message + " - returning null.");
            return null;
        }
    }
}