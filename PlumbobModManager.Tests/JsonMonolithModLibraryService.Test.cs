using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

public class JsonMonolithModLibraryService_Test
{
    private const string BASE_DIR = TestUtils.PMM_UNIT_TEST_BASE_DIR;

    [SetUp]
    public void Setup()
    {
        //reset with a fresh directory for testing
        if(Directory.Exists(BASE_DIR))
        {
            Directory.Delete(BASE_DIR, true);
        }
        Directory.CreateDirectory(BASE_DIR);
    }

    [TearDown]
    public void TearDown()
    {
        if(Directory.Exists(BASE_DIR))
        {
            Directory.Delete(BASE_DIR, true);
        }
    }
    
    [Test]
    public void SaveToFile_NewValidFileTest()
    {
        //setup
        string testFile = "test.json";
        string testDir = BASE_DIR;
        string testpath = Path.Combine(testDir, testFile);
        
        var lib = new JsonMonolithModLibraryService();
        
        //test
        lib.SaveToFile(testpath);
        
        var savedFile = File.ReadAllText(testpath);
        
        Assert.Multiple(() => {
            Assert.That(savedFile, Is.Not.Null, 
                "Saved file should not be null");
            Assert.That(savedFile, Is.Not.Empty, 
                "Saved file should not be empty");
            Assert.That(savedFile, Is.Not.EqualTo(""),
                "Saved file should not be empty string");
            Assert.That(savedFile.Length, Is.GreaterThan(0), 
                "Saved file should have non-zero length");
            
            Assert.That(savedFile, Is.EqualTo(lib.Serialize()), 
                "Saved file should match serialized library");
        });
        
    }

    [Test]
    public void SaveToFile_ExistingFileOverwriteTest()
    {
        string testFile = "test.json";
        string testDir = BASE_DIR;
        string testpath = Path.Combine(testDir, testFile);
        File.WriteAllText(testpath, "TEST_STRING");

        var lib = new JsonMonolithModLibraryService();
        lib.SaveToFile(testpath);

        var savedFile = File.ReadAllText(testpath);
        Assert.That(savedFile, Is.EqualTo(lib.Serialize()),
            "Saved file should match serialized library");

        Assert.That(savedFile, Is.Not.Null,
            "Saved file should not be null");
        
        Assert.Multiple(() => {
            Assert.That(savedFile, Is.Not.EqualTo("TEST_STRING"),
                "Saved file should have overwritten the original contents");

            Assert.That(savedFile, Is.Not.Empty,
                "Saved file should not be empty");

            Assert.That(savedFile, Is.Not.EqualTo(""),
                "Saved file should not be empty string");

            Assert.That(savedFile.Length, Is.GreaterThan(0),
                "Saved file should have non-zero length, even without any elements in the library.");
        });
    }

    [Test]
    public void Json_ReserializeTest()
    {
        string testFile = "test.json";
        string testDir = BASE_DIR;
        string testpath = Path.Combine(testDir, testFile);
        File.WriteAllText(testpath, "TEST_STRING");
        
        //create the lib
        var lib = new JsonMonolithModLibraryService();
        
        //add a test mod to it
        var testModEntry = new ModEntry("fake mod path", null);
        lib.TryAddMod(testModEntry);
        
        Assert.That(lib.ModList.Count, Is.EqualTo(1), 
            "Library should contain exactly one mod entry after adding only one mod entry");
        
        //save it to disk
        lib.SaveToFile(testpath);       
        
        //read it back in
        var reloadedLib = JsonMonolithModLibraryService.Deserialize(
            File.ReadAllText(testpath));
        
        //verify that it's the same
        Assert.That(reloadedLib, Is.Not.Null, 
            "Deserialized library should not be null");
        
        Assert.That(reloadedLib.ModList, Is.Not.Null, 
            "Deserialized library mod list should not be null");
        
        Assert.That(reloadedLib.ModList.Count, Is.EqualTo(1), 
            "Deserialized library should contain exactly one mod entry");
        
        Assert.That(reloadedLib.ModList[0], Is.EqualTo(testModEntry), 
            "Deserialized library should contain the same single mod entry, and being a record, should be structurally equal and evaluate as equal.");
            
    }
    
    [Test]
    public void AddMod_CopyModTest()
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
        
        // string expectedModSlugString = ""
        
        //quick check that the test setup is it self correct
        Assert.That(testModDirInfo.Exists, Is.True, 
            "Test mod directory should exist");
        
        //create the lib
        var lib = new JsonMonolithModLibraryService();
        
        Assert.That(lib, Is.Not.Null, "Library should be created successfully");
        
        //add a test mod to it
        var testModEntry = new ModEntry(testModDirPath, null);
        lib.TryAddMod(testModEntry);
        
        Assert.That(lib.ModList, Has.Count.EqualTo(1), "Library count should reflect the added mod");
    }
}