using System.Text.Json;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

public class AppConfigTests
{
    private static readonly string MODLIB_PATH = @"D:\Modding\Sims 4\PlumbobMM\library".Replace(@"\", @"\\");
    private static readonly string RIGS_ROOT_PATH = @"D:\Modding\Sims 4\PlumbobMM\rigs".Replace(@"\", @"\\");
    private const string FULL_VERSION_STRING = "1.12.133-Alpha";
    private const string SHORT_VERSION_STRING = "1.12.133";
    private const string FULL_APP_NAME = "Testing TS4 Plumbob Mod Manager";
    private const string SHORT_APP_NAME = "TS4PlumbobMM";
    
    private string SIMPLE_CONFIG_JSON => $$"""
        {
          "userSettings": {
            "modLibraryPath": "{{MODLIB_PATH}}",
            "rigsRootPath": "{{RIGS_ROOT_PATH}}"
          },
          "version": "{{FULL_VERSION_STRING}}",
          "shortVersion": "{{SHORT_VERSION_STRING}}",
          "appName": "{{FULL_APP_NAME}}",
          "shortAppName": "{{SHORT_APP_NAME}}"
        }
        """;
    
    [SetUp]
    public void Setup()
    {
        ServiceLocator.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        ServiceLocator.Reset();
    }
    
    [Test]
    public void AppConfig_UserSettings_ShouldPopulateCorrectly()
    {
        // Given
        string json = SIMPLE_CONFIG_JSON;

        // When
        var config = JsonSerializer.Deserialize<AppConfig>(json, AppConfig.AppSerializerOptions);

        // Then
        Assert.That(config, Is.Not.Null, 
            "AppConfig should not be null");
        
        Assert.That(config.FullVersionString, Is.EqualTo(FULL_VERSION_STRING), "FullVersionString was not deserialized correctly");
        
        Assert.That(config.FullAppName, Is.EqualTo(FULL_APP_NAME), "FullAppName was not deserialized correctly");
        
        Assert.That(config.ShortVersionString, Is.EqualTo(SHORT_VERSION_STRING), "ShortVersionString was not deserialized correctly");
        
        Assert.That(config.ShortAppName, Is.EqualTo(SHORT_APP_NAME), "ShortAppName was not deserialized correctly");

        Assert.That(config.UserSettings, Is.Not.Null, "UserSettings should not be null");
        
        //I feel like there's a better way than just swapping between \ and \\
        Assert.That(config.UserSettings.ModLibraryPath, Is.EqualTo(MODLIB_PATH.Replace(@"\\", @"\")),
            "ModLibraryPath was not deserialized correctly");
        Assert.That(config.UserSettings.RigsRootPath, Is.EqualTo(RIGS_ROOT_PATH.Replace(@"\\", @"\")),
            "RigsRootPath was not deserialized correctly");
    }       
}