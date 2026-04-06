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
    [JsonInclude, JsonPropertyName("profileRootPath")]
    public string ProfileRootPath { get; set; }

    [JsonInclude, JsonPropertyName("modLibraryPath")]
    public string ModLibraryPath { get; set; }
}

/// <summary>
/// Top-level application config class. All app-wide settings and
/// configurations can be found here or on the delegated class properties. 
/// </summary>
public class AppConfig : IService
{
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
    public string ShortAppName { get; set; } = "PMM";

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
        string serialized = JsonSerializer.Serialize(this);
        File.WriteAllText(MainAppConfig, serialized);
    }

    public static AppConfig? LoadFromDisk()
    {
        try
        {
            string fileText = File.ReadAllText(MainAppConfig);
            return JsonSerializer.Deserialize<AppConfig>(fileText);
        }
        catch (DirectoryNotFoundException e)
        {
            Debug.WriteLine(
                $"AppConfig.LoadFromDisk failed - a directory was missing in " +
                $"path \"{MainAppConfig}\" : " + e.Message + " - returning null.");
            return null;
        }
        catch (FileNotFoundException e)
        {
            Debug.WriteLine(
                $"AppConfig.LoadFromDisk failed - file \"{MainAppConfig}\" not " +
                $"present: " + e.Message + " - returning null.");
            return null;
        }
    }

    
}