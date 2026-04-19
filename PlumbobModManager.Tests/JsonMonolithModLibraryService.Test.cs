using System.Diagnostics;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

[TestFixture]
public class JsonMonolithModLibraryService_Test : AbstractPlumbobTest
{
    //not nullable within tests 
    private static Mod _testMod;
    public abstract class AbstractModLibServiceTest : AbstractPlumbobTest
    {
        [SetUp]
        public void ClassTestSetup()
        {
            Console.WriteLine("[JsonMonolithModLibraryService_Test Setup] setting up servicelocator");
            ServiceLocator.Reset(); //ensure a clean slate to start from
            
            var testAppConfig = new AppConfig {
                UserSettings = {
                    ModLibraryPath = BASE_DIR
                }
            };
            ServiceLocator.Register<AppConfig>(testAppConfig);

            Console.WriteLine("[JsonMonolithModLibraryService_Test Setup] setting up _testMod");
            _testMod = new Mod(
                new ModSlug("test-slug", 0),
                new AuthorProfile(
                    "Mr. Test",
                    ["https://x.com/mrmod"],
                    "https://mrmods.com"
                ));
        }
        
        [TearDown]
        public void ClassTestTeardown()
        {
            Console.WriteLine("JsonMonolithModLibraryService_Test TearDown - tearing down servicelocator");
            ServiceLocator.Reset(); //ensure a clean slate to start from
            Console.WriteLine("JsonMonolithModLibraryService_Test TearDown - tearing down _testMod");
            //nullified to prevent memory leaks
            //within all test cases, it is guaranteed to not be null due to setup.
            _testMod = null!; 
        }
    }
    
    public class Init : AbstractModLibServiceTest
    {
        [Test]
        public void LibInit_Test()
        {
            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            
            Assert.That(lib, Is.Not.Null, 
                "Library should be created successfully");
            Assert.That(lib.ModList, Is.Not.Null, 
                "ModList should not be null");
            Assert.That(lib.ModList.Count, Is.EqualTo(0), 
                "ModList should be empty");
            Assert.That(lib.Rigs, Is.Not.Null, 
                "Rigs list should not be null");
            Assert.That(lib.ActiveRig, Is.Not.Null, 
                "ActiveRig should be assigned after registration due to CreateDefaultRigIfNone.");
            Assert.That(lib.Rigs, Has.Count.EqualTo(1), 
                "Rigs list should contain one rig after registration due to CreateDefaultRigIfNone.");
        }
        
        [Test]
        public async Task SaveToFile_NewValidFileTest()
        {
            //setup
            string testFile = "test.json";
            string testDir = BASE_DIR;
            string testpath = Path.Combine(testDir, testFile);
            
            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            
            //test
            await lib.SaveToFileAsync(testpath);
            
            var savedFile = await File.ReadAllTextAsync(testpath);
            string serializedLib = await lib.SerializeAsync();
            Assert.Multiple(() => {
                Assert.That(savedFile, Is.Not.Null, 
                    "Saved file should not be null");
                Assert.That(savedFile, Is.Not.Empty, 
                    "Saved file should not be empty");
                Assert.That(savedFile, Is.Not.EqualTo(""),
                    "Saved file should not be empty string");
                Assert.That(savedFile.Length, Is.GreaterThan(0), 
                    "Saved file should have non-zero length");

                
                Assert.That(savedFile, Is.EqualTo(serializedLib), 
                    "Saved file should match serialized library");
            });
            
        }

        [Test]
        public async Task SaveToFile_ExistingFileOverwriteTest()
        {
            string testFile = "test.json";
            string testDir = BASE_DIR;
            string testpath = Path.Combine(testDir, testFile);
            
            Assert.That(testFile, Is.Not.Null,
                "Test file name should not be null");
            Assert.That(testDir, Is.Not.Null,
                "Test directory should not be null");
            
            Assert.That(Directory.Exists(testDir), Is.True,
                $"Test directory '{testDir}' should exist for testing");

            Assert.That(File.Exists(testpath), Is.False,
                $"Test file '{testpath}' should not already exist before testing");
            
            Assert.That(testpath, Is.Not.Null,
                "Test path should not be null");
            
            await File.WriteAllTextAsync(testpath, "TEST_STRING");

            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            await lib.SaveToFileAsync(testpath);

            var savedFileText = await File.ReadAllTextAsync(testpath);
            var serializedLib = await lib.SerializeAsync();
            Assert.That(savedFileText, Is.EqualTo(serializedLib),
                "Saved file should match serialized library");

            Assert.That(savedFileText, Is.Not.Null,
                "Saved file should not be null");
            
            Assert.Multiple(() => {
                Assert.That(savedFileText, Is.Not.EqualTo("TEST_STRING"),
                    "Saved file should have overwritten the original contents");

                Assert.That(savedFileText, Is.Not.Empty,
                    "Saved file should not be empty");

                Assert.That(savedFileText, Is.Not.EqualTo(""),
                    "Saved file should not be empty string");

                Assert.That(savedFileText.Length, Is.GreaterThan(0),
                    "Saved file should have non-zero length, even without any elements in the library.");
            });
        }
    }

    public class Service : AbstractModLibServiceTest
    {
        // [SetUp]
        // public void SetUpServiceLocator()
        // {
        //     Console.WriteLine("Test class Service - setting up fresh ServiceLocator");
        //     ServiceLocator.Reset(); //ensure a clean slate to start from
        // } 
        //
        // [TearDown]
        // public void TearDownServiceLocator()
        // {
        //     Console.WriteLine("Test class Service - tearing down ServiceLocator");
        //     ServiceLocator.Reset(); //ensure anything the test did is cleaned up
        // }
        
        [Test]
        public void RegisterService_Test()
        {
            var lib = new JsonMonolithModLibraryService();
            Assert.That(lib, Is.Not.Null,
                "library should not be null after creation");
            Console.WriteLine("registering lib " + lib);
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);

            Assert.That(result, Is.Not.Null, 
                $"The {nameof(JsonMonolithModLibraryService)} should " +
                $"be registerable as a {nameof(IModLibraryService)}. " +
                $"Make sure it is a valid type for this.");
            Assert.That(result, Is.EqualTo(lib), 
                "A successful service registration should return the " +
                "registered service instance");
            
            Console.WriteLine("registered lib " + lib);
            
            var serviceLib = ServiceLocator.Resolve<IModLibraryService>();

            Assert.That(lib, Is.Not.Null,
                "library should not be null after registration");
            Assert.That(serviceLib, Is.Not.Null,
                $"ServiceLocator should resolve the registered service " +
                $"from the type it was registered with " +
                $"({nameof(IModLibraryService)} in this case.)");
            Assert.That(serviceLib, Is.EqualTo(lib),
                "ServiceLocator should resolve the registered service " +
                "to the same instance");
            
            Assert.Multiple(() =>
            {
                Assert.That(serviceLib.ActiveRig, Is.Not.Null,
                    "A registered library service should have an ActiveRig - " +
                    "if there are no registered rigs, " +
                    "one should be created automatically");
                Assert.That(serviceLib.Rigs, Is.Not.Null,
                    "Rigs list should be initialized");
            });
            
            Assert.That(serviceLib.Rigs, Has.Count.EqualTo(1),
                "The Rigs list of an empty but registered library service " +
                "should contain one rig by default");
            Assert.That(serviceLib.Rigs[0], Is.Not.Null,
                "Rig 0 should be initialized in an empty but registered library");
            Assert.That(serviceLib.Rigs[0], Is.EqualTo(serviceLib.ActiveRig),
                "The sole initial rig in an empty but registered library " +
                "should be the active rig.");
            
            ServiceLocator.TryUnregister<IModLibraryService>(serviceLib);
            
            Assert.That(ServiceLocator.Resolve<IModLibraryService>(), Is.Null,
                "ServiceLocator should not be able to resolve the " +
                "unregistered service when there is no bound jumpstarter.");
            
            Assert.That(serviceLib.ActiveRig, Is.Not.Null,
                "An unregistered library service should not clear its " +
                "ActiveRig when deregistered.");

            ServiceLocator.Register<IModLibraryService>(lib);
             
            var serviceLib2 = ServiceLocator.Resolve<IModLibraryService>();
            
            Assert.That(serviceLib2, Is.Not.Null,
                "ServiceLocator should resolve the registered service");
            Assert.That(serviceLib2, Is.EqualTo(lib),
                "ServiceLocator should resolve the registered service " +
                "to the same instance");
            Assert.That(serviceLib2, Is.EqualTo(serviceLib),
                "ServiceLocator should still resolve the registered service " +
                "to the same instance even after being unregistered and " +
                "re-registered");

            Assert.Multiple(() =>
            {
                Assert.That(serviceLib2.ActiveRig, Is.Not.Null,
                    "A re-registered library service should have an ActiveRig - " +
                    "and in this case, one should already exist");
                Assert.That(serviceLib2.Rigs, Is.Not.Null,
                    "Rigs list should still be initialized after re-registering");
            });

            Assert.That(serviceLib2.Rigs, Has.Count.EqualTo(1),
                "The Rigs list of an empty but re-registered library service " +
                "should still contain exactly one rig after re-registering");
            Assert.That(serviceLib2.Rigs[0], Is.Not.Null,
                "Rig 0 should still be initialized in an empty but re-registered library");
            Assert.That(serviceLib2.Rigs[0], Is.EqualTo(serviceLib2.ActiveRig),
                "The sole initial rig in an empty but re-registered library " +
                "should still be the active rig.");
        }
    }

    public class ModRelatedTests : AbstractModLibServiceTest
    {
        [Test]
        public async Task Json_CircularReferenceTest()
        {
            string testFile = "circular_test.json";
            string testDir = BASE_DIR;
            string testpath = Path.Combine(testDir, testFile);
            
            Assert.That(testpath, Is.Not.Null,
                "Test file path should be valid");
            
            //create the lib
            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            
            //create a Mod and a ModEntry that point to each other
            ModEntry entry = _testMod.AddNewEntry(new Version(1, 0), "fake");
            lib.TryAddMod(_testMod);
            
            //save it
            await lib.SaveToFileAsync(testpath);
            
            string savedJson = await File.ReadAllTextAsync(testpath);
            Console.WriteLine("[DEBUG_LOG] Saved Circular JSON: " + savedJson);
            
            //read it back
            var reloadedLib = await JsonMonolithModLibraryService.DeserializeAsync(savedJson);
            reloadedLib?.InitializeFromSerializedData();
            
            Assert.That(reloadedLib, Is.Not.Null, "Reloaded library should not be null");
            Assert.That(reloadedLib.ModList, Has.Count.EqualTo(1));
            Mod reloadedMod = reloadedLib.ModList[0];
            Assert.That(reloadedMod, Is.Not.Null);
            Assert.That(reloadedMod.MetadataTemplate, Is.EqualTo(_testMod.MetadataTemplate), "Mod Metadata template should be equivalent after reserialization");
            Assert.That(reloadedMod.Entries.First(), Is.EqualTo(entry), "The first (and only) element in the reserialized Entries set should point back to the same ModEntry instance");
            Assert.That(reloadedMod.Entries.First(), Is.EqualTo(_testMod.Entries.First()),
                "The first (and only) element in the reserialized Entries set should point back to the same element in the original Mod's Entries set.");
        }

        [Test]
        public async Task Json_ReserializeTest()
        {
            string testFile = "test.json";
            string testDir = BASE_DIR;
            string testpath = Path.Combine(testDir, testFile);
            await File.WriteAllTextAsync(testpath, "TEST_STRING");
            
            //create the lib
            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            
            //add a test mod to it
            
            _testMod.AddNewEntry(new Version(1, 0), "fake");
            lib.TryAddMod(_testMod);
            
            Assert.That(lib.ModList, Has.Count.EqualTo(1), 
                "Library should contain exactly one mod entry after adding only one mod entry");
            
            //save it to disk
            await lib.SaveToFileAsync(testpath);       
            
            //read it back in
            var reloadedLib = await JsonMonolithModLibraryService.DeserializeAsync(
                await File.ReadAllTextAsync(testpath));
            reloadedLib?.InitializeFromSerializedData();
            
            //verify that it's the same
            Assert.That(reloadedLib, Is.Not.Null, 
                "Deserialized library should not be null");
            
            Assert.That(reloadedLib.ModList, Is.Not.Null, 
                "Deserialized library mod list should not be null");
            
            Assert.That(reloadedLib.ModList, Has.Count.EqualTo(1), 
                "Deserialized library should contain exactly one mod entry");
            
            Assert.That(reloadedLib.ModList[0], Is.EqualTo(_testMod), 
                "Deserialized library should contain the same single mod entry, and being a record, should be structurally equal and evaluate as equal.");
            
            Assert.That(reloadedLib.ModList[0], Is.EqualTo(lib.ModList[0]), 
                "Deserialized library should contain the same single mod entry, and being a record, should be structurally equal and evaluate as equal.");
                
        }

        public class ModAddTests : AbstractModLibServiceTest
        {
            private const string TEST_MOD_DIR_BASE_NAME = "TestMod";
            private static readonly string _testModDirPath = Path.Combine(BASE_DIR, TEST_MOD_DIR_BASE_NAME);

            private readonly string _testPackage1Path = Path.Combine(
                _testModDirPath, "testpackage1.package");
            private readonly byte[] _testPackage1Bytes = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

            private readonly string _testPackage2Path = Path.Combine(
                _testModDirPath, "testpackage2.package");
            private readonly byte[] _testPackage2Bytes = [11, 12, 13, 14, 15, 16, 17, 18, 19, 20];

            private readonly string _testTs4ScriptPath = Path.Combine(
                _testModDirPath, "test.ts4script");
            private readonly byte[] _testTs4ScriptBytes = [21, 22, 23, 24, 25, 26, 27, 28, 29, 30];

            private readonly string _testReadmePath = Path.Combine(
                _testModDirPath, "README.txt");

            private const string TEST_README_TEXT = "This is a test mod package";

            DirectoryInfo _testModDirInfo;

            [SetUp]
            public void Setup()
            {
                Console.WriteLine(
                    "[Test class ModAddTests] setting up test " +
                    "folder/files (and service locator)");
                
                //create fake test mod package in PMM_UNIT_TEST_BASE_DIR
                _testModDirInfo = Directory.CreateDirectory(_testModDirPath);

                //populate it with some files
                File.WriteAllBytes(_testPackage1Path, _testPackage1Bytes);
                File.WriteAllBytes(_testPackage2Path, _testPackage2Bytes);
                File.WriteAllBytes(_testTs4ScriptPath, _testTs4ScriptBytes);
                File.WriteAllText(_testReadmePath, TEST_README_TEXT);
            }
            
            [TearDown]
            public void TearDown()
            {
                Console.WriteLine(
                    "Test class ModAddTests - tearing down " +
                    "test folder/files (and service locator)");
                if (Directory.Exists(_testModDirPath))
                {
                    Directory.Delete(_testModDirPath, true);
                }
            }
            
            [Test]
            public void AddMod_CopyModTest()
            {
                //quick check that the test setup is itself correct
                Assert.That(_testModDirInfo.Exists, Is.True,
                    "Test mod directory should exist");

                //create the lib
                var lib = new JsonMonolithModLibraryService();
                IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
                 
                Assert.That(lib.ActiveRig, Is.Not.Null,
                    "ActiveRig should not be null after " +
                    "library registration.");

                Assert.That(lib, Is.Not.Null, 
                    "Library should be created successfully");

                //add a test mod to it
                // ModEntry entry = _testMod.AddNewEntry(new Version(1, 0), "fake");
                lib.TryAddMod(_testMod);

                Assert.That(lib.ModList, Has.Count.EqualTo(1), 
                    "Library count should reflect the added mod");

                Console.WriteLine("Mod 0: " + lib.ModList[0]);

                //need to first add the mod to a rig before it vis
                var v = lib.GetVisibleMods().ToArray();
                
                Assert.That(v, Is.Not.Null, 
                    "Visible mods array/enumerable should not " +
                    "be null after adding a mod");
                Assert.That(v, Is.Empty, 
                    "We have not yet added the mod to a rig, " +
                    "only the library, so it is not visible.");
            }
            
            [Test]
            public void AddModToRig_CopyModTest()
            {
                Console.WriteLine("Beginning Test class ModAddTests - AddModToRig_CopyModTest");
                //quick check that the test setup is itself correct
                Assert.That(_testModDirInfo.Exists, Is.True,
                    "Test mod directory should exist");

                //create the lib
                var lib = new JsonMonolithModLibraryService();
                IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
                
                Assert.That(lib.ActiveRig, Is.Not.Null,
                    "ActiveRig should not be null " +
                    "after library registration.");

                Assert.That(lib, Is.Not.Null, 
                    "Library should be created successfully");

                Assert.That(ServiceLocator.Resolve<AppConfig>(), Is.Not.Null, "AppConfig should have been registered during setup!");
                //add a test mod to it
                var testModEntry = _testMod.AddNewEntry(new Version(13,1,56,6));
                lib.TryAddMod(_testMod);

                var slug = testModEntry.Slug;
                Assert.That(slug, Is.Not.Null, 
                    "Test Check - Mod should have a slug");
                Assert.That(slug.ToString(), Is.Not.Empty, 
                    "Test Check - Mod slug should not be empty");
                
                try
                {
                    lib.ActiveRig?.TryAddModEntryToEnd(testModEntry);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(
                        $"Got expected exception when attempting to " +
                        $"add a non-library mod to a rig: {ex}");
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"Unexpected exception when attempting to " +
                        $"add a non-library mod to a rig: {ex}");
                }
                
                // lib.TryAddModEntry(testModEntry);

                // ModEntry entry = testMod.AddNewEntry(new Version(1, 0), "fake");
                
                
                Assert.That(lib.ModList, Has.Count.EqualTo(1), 
                    "Library count should reflect the added mod");
                Console.WriteLine("Mod 0: " + lib.ModList[0]);
                
                lib.ActiveRig?.TryAddModEntryToEnd(testModEntry);
                
                //need to first add the mod to a rig before it vis
                var v = lib.GetVisibleMods().ToArray();
                Assert.That(v, Is.Not.Null, 
                    "Visible mods array/enumerable should not be " +
                    "null after adding a mod");
                Assert.That(v, Is.Not.Empty, 
                    "We have not yet added the mod to a rig, " +
                    "only the library, so it is not visible.");
                Assert.That(v.Length, Is.EqualTo(1), 
                    "There should be exactly 1 visible mod " +
                    "in the active rig after adding only one.");
            }
        }
    }
    
    public class RigTests : AbstractModLibServiceTest
    {
        [Test]
        public void GetVisibleMods_CountTest()
        {
            //create the lib
            var lib = new JsonMonolithModLibraryService();
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);
            // lib.OnRegister(lib.GetType());
            Assert.That(lib.ActiveRig, Is.Not.Null, 
                "ActiveRig should not be null after adding a mod");

            Assert.That(lib.GetVisibleMods, Is.Not.Null, "Visible mods collection should be initialized");
            Assert.That(lib.GetVisibleMods().Count, Is.EqualTo(0), "Visible mods collection should be empty initially");

            // var newEntry = ModEntry.CreateNewUnique("testpath");
            // lib.TryAddMod()
            lib.TryAddMod(_testMod);
            var newEntry = _testMod.AddNewEntry(new Version(1, 0), "fake");
            lib.ActiveRig?.TryAddModEntryToEnd(newEntry);
            
            Assert.That(lib.ActiveRig, Is.Not.Null,
                "ActiveRig should not be null after adding a mod");
            Assert.That(lib.GetVisibleMods().Count, Is.EqualTo(1), "Visible mods collection should contain exactly 1 mod after adding it");
            
            lib.ActiveRig?.TryAddModEntryToEnd(newEntry);
            Assert.That(lib.ActiveRig, Is.Not.Null,
                "ActiveRig should not be null after adding a mod");
            Assert.That(lib.GetVisibleMods().Count, Is.EqualTo(1), "Visible mods collection should still contain exactly 1 mod after adding it again");
        }
    }
}