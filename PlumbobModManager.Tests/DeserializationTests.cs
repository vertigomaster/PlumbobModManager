using System.Text.Json;
using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

public class DeserializationTests
{
    [Test]
    public void AppConfig_UserSettings_ShouldPopulateCorrectly()
    {
        // Given
        string json = @"{
          ""userSettings"": {
            ""modLibraryPath"": ""D:\\Modding\\Sims 4\\PlumbobMM\\library"",
            ""profileRootPath"": ""D:\\Modding\\Sims 4\\PlumbobMM\\rigs""
          },
          ""version"": ""1.12.133-Alpha"",
          ""shortVersion"": ""1.12.133"",
          ""appName"": ""TS4 Plumbob Mod Manager MODIFIED"",
          ""shortAppName"": ""TS4PlumbobMM"",
        }";

        // When
        var config = JsonSerializer.Deserialize<AppConfig>(json, AppConfig.AppSerializerOptions);

        // Then
        Assert.That(config, Is.Not.Null);
        Assert.That(config.FullVersionString, Is.EqualTo("1.12.133"));
        Assert.That(config.FullAppName, Is.EqualTo("TS4 Plumbob Mod Manager MODIFIED"));

        Assert.That(config.UserSettings, Is.Not.Null, "UserSettings should not be null");
        Assert.That(config.UserSettings.ModLibraryPath, Is.EqualTo("D:\\Modding\\Sims 4\\PlumbobMM\\library"),
            "ModLibraryPath was not deserialized correctly");
        Assert.That(config.UserSettings.RigsRootPath, Is.EqualTo("D:\\Modding\\Sims 4\\PlumbobMM\\rigs"),
            "RigsRootPath was not deserialized correctly");
    }
}