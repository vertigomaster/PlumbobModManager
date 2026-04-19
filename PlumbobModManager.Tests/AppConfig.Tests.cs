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

public class ModEntryTests : AbstractPlumbobTest
{
    private const string BASE_DIR = TestUtils.PMM_UNIT_TEST_BASE_DIR;
    private const string TEST_MOD_DIR_NAME = "TestMod";
    private const string TEST_MOD_NAME = "Test Mod";
    private string TestModPath => Path.Combine(BASE_DIR, TEST_MOD_DIR_NAME);
    
    // [SetUp]
    // public void Setup()
    // {
    //     //reset with a fresh directory for testing
    //     if (Directory.Exists(BASE_DIR))
    //     {
    //         Directory.Delete(BASE_DIR, true);
    //     }
    //
    //     Directory.CreateDirectory(BASE_DIR);
    // }
    //
    // [TearDown]
    // public void TearDown()
    // {
    //     if (Directory.Exists(BASE_DIR))
    //     {
    //         Directory.Delete(BASE_DIR, true);
    //     }
    // }
    
    [Test]
    public void ModDiskTest()
    {
        string testModDirBaseName = "TestMod";
        string testModDirPath = Path.Combine(BASE_DIR, testModDirBaseName);
        
        //create fake test mod package in PMM_UNIT_TEST_BASE_DIR
        DirectoryInfo testModDirInfo = Directory.CreateDirectory(testModDirPath);
        
        string testPackage1Path = Path.Combine(testModDirPath, "testpackage1.package");
        byte[] testPackage1Bytes = [1,2,3,4,5,6,7,8,9,10];
        
        string testPackage2Path = Path.Combine(testModDirPath, "testpackage2.package");
        byte[] testPackage2Bytes = [11,12,13,14,15,16,17,18,19,20];
        
        string testTs4ScriptPath = Path.Combine(testModDirPath, "test.ts4script");
        byte[] testTs4ScriptBytes = [21,22,23,24,25,26,27,28,29,30];       
        
        string testReadmePath = Path.Combine(testModDirPath, "README.txt");
        string testReadmeText = "This is a test mod package";
        
        // string expectedModSlug = ModSlug.SanitizeForSlug(testModDirBaseName);
        
        //populate it with some files
        File.WriteAllBytes(testPackage1Path, testPackage1Bytes);
        File.WriteAllBytes(testPackage2Path, testPackage2Bytes);
        File.WriteAllBytes(testTs4ScriptPath, testTs4ScriptBytes);
        File.WriteAllText(testReadmePath, testReadmeText);
        
        // Setup service locator for ModEntry.ExistsOnDisk
        var appConfig = new AppConfig();
        appConfig.UserSettings.ModLibraryPath = BASE_DIR;
        IDEK.Tools.ShocktroopUtils.Services.ServiceLocator.Register<AppConfig>(appConfig);
        
        var lib = new JsonMonolithModLibraryService();
        IDEK.Tools.ShocktroopUtils.Services.ServiceLocator.Register<IModLibraryService>(lib);

        // string expectedModSlugString = ""
        
        //quick check that the test setup is it self correct
        Assert.That(testModDirInfo.Exists, Is.True, 
            "Test mod directory should exist");
        
        Assert.That(testModDirInfo.GetFiles().Length, Is.EqualTo(4), 
            "Test mod directory should contain exactly four files");

        var mod = new Mod(
            new ModMetadata(
                TEST_MOD_NAME,
                new Version("1.0.0"),
                new AuthorProfile("SomeGuy", ["www.someguy.com"])
            ));
        
        var testModEntry = mod.AddNewEntry(new Version(1,0));
        
        Assert.That(testModEntry, Is.Not.Null, 
            "ModEntry should not be null");
        Console.WriteLine($"testModEntry '{testModEntry}' " +
            $"can be found at '{testModEntry.AbsPath}'");
        Assert.That(testModEntry.ExistsOnDisk, Is.True, 
            $"ModEntry should exist on disk (specifically, '{testModEntry.AbsPath}' should exist during the test)");       
        
    }
}