using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

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
        
        var lib = new JsonMonolithAsyncModLibraryService();
        IDEK.Tools.ShocktroopUtils.Services.ServiceLocator.Register<IAsyncModLibraryService>(lib);

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