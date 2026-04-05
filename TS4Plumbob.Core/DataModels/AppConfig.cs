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
    public static string AppFolder => AppContext.BaseDirectory;
    public static string AppConfigDir => Path.Combine(AppFolder, "config");
    public static string MainAppConfig => Path.Combine(AppConfigDir, "appconfig.json");

    #region Implementation of IService

    /// <inheritdoc />
    public void OnRegister(Type type) { }

    /// <inheritdoc />
    public void OnUnregister(Type type)
    {
        SaveToDisk();
    }

    #endregion
    
    public void SaveToDisk()
    {
        string serialized = JsonSerializer.Serialize(this);
        File.WriteAllText(MainAppConfig, serialized);
    }

    public static AppConfig? LoadFromDisk()
    {
        string fileText = File.ReadAllText(MainAppConfig);
        return JsonSerializer.Deserialize<AppConfig>(fileText);
    }

    
}