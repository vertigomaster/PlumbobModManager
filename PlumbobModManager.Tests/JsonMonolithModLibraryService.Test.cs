using System.Diagnostics;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

[TestFixture]
public class JsonMonolithModLibraryService_Test : AbstractPlumbobTest
{
    public class Init : AbstractPlumbobTest
    {
        [Test]
        public void LibInit_Test()
        {
            var lib = new JsonMonolithModLibraryService();
            
            Assert.That(lib, Is.Not.Null, 
                "Library should be created successfully");
            Assert.That(lib.ModList, Is.Not.Null, 
                "ModList should not be null");
            Assert.That(lib.ModList.Count, Is.EqualTo(0), 
                "ModList should be empty");
            Assert.That(lib.Rigs, Is.Not.Null, 
                "Rigs list should not be null");
            Assert.That(lib.ActiveRig, Is.Null, 
                "ActiveRig should not yet be assigned - " +
                "serializers need a fully blank state to " +
                "deserialize data into.");
            Assert.That(lib.Rigs, Has.Count.EqualTo(0), 
                "Rigs list should be empty on a blank library");
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
    }

    public class Service : AbstractPlumbobTest
    {
        [SetUp]
        public void SetUpServiceLocator()
        {
            ServiceLocator.Reset(); //ensure a clean slate to start from
        } 
        
        [TearDown]
        public void TearDownServiceLocator()
        {
            ServiceLocator.Reset(); //ensure anything the test did is cleaned up
        }
        
        [Test]
        public void RegisterService_Test()
        {
            var lib = new JsonMonolithModLibraryService();
            Assert.That(lib, Is.Not.Null,
                "library should not be null after creation");
            Debug.WriteLine("registering lib " + lib);
            IModLibraryService? result = ServiceLocator.Register<IModLibraryService>(lib);

            Assert.That(result, Is.Not.Null, 
                $"The {nameof(JsonMonolithModLibraryService)} should " +
                $"be registerable as a {nameof(IModLibraryService)}. " +
                $"Make sure it is a valid type for this.");
            Assert.That(result, Is.EqualTo(lib), 
                "A successful service registration should return the " +
                "registered service instance");
            
            Debug.WriteLine("registered lib " + lib);
            
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

    public class ModRelatedTests : AbstractPlumbobTest
    {
        [Test]
        public void Json_CircularReferenceTest()
        {
            string testFile = "circular_test.json";
            string testDir = BASE_DIR;
            string testpath = Path.Combine(testDir, testFile);
            
            Assert.That(testpath, Is.Not.Null,
                "Test file path should be valid");
            
            //create the lib
            var lib = new JsonMonolithModLibraryService();
            
            //create a Mod and a ModEntry that point to each other
            var modEntry = new ModEntry("fake mod path", null);
            var mod = new Mod { Slug = new ModSlug("test-slug", 0), Template = modEntry };
            modEntry.ModConcept = mod;
            mod.Entries.Add(modEntry);
            
            lib.TryAddMod(modEntry);
            
            //save it
            lib.SaveToFile(testpath);
            
            string savedJson = File.ReadAllText(testpath);
            Console.WriteLine("[DEBUG_LOG] Saved Circular JSON: " + savedJson);
            
            //read it back
            var reloadedLib = JsonMonolithModLibraryService.Deserialize(savedJson);
            
            Assert.That(reloadedLib.ModList.Count, Is.EqualTo(1));
            var reloadedEntry = reloadedLib.ModList[0];
            Assert.That(reloadedEntry.ModConcept, Is.Not.Null);
            Assert.That(reloadedEntry.ModConcept.Template, Is.SameAs(reloadedEntry), "Template should point back to the same ModEntry instance");
            Assert.That(reloadedEntry.ModConcept.Entries[0], Is.SameAs(reloadedEntry), "Entries[0] should point back to the same ModEntry instance");
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

        public class ModAddTests : AbstractPlumbobTest
        {
            static string testModDirBaseName = "TestMod";
            static string testModDirPath = Path.Combine(BASE_DIR, testModDirBaseName);

            string testPackage1Path = Path.Combine(testModDirPath, "testpackage1.package");
            byte[] testPackage1Bytes = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

            string testPackage2Path = Path.Combine(testModDirPath, "testpackage2.package");
            byte[] testPackage2Bytes = [11, 12, 13, 14, 15, 16, 17, 18, 19, 20];

            string testTs4ScriptPath = Path.Combine(testModDirPath, "test.ts4script");
            byte[] testTs4ScriptBytes = [21, 22, 23, 24, 25, 26, 27, 28, 29, 30];

            string testReadmePath = Path.Combine(testModDirPath, "README.txt");
            string testReadmeText = "This is a test mod package";

            DirectoryInfo testModDirInfo;

            [SetUp]
            public void Setup()
            {
                ServiceLocator.Reset();
                
                //create fake test mod package in PMM_UNIT_TEST_BASE_DIR
                testModDirInfo = Directory.CreateDirectory(testModDirPath);

                //populate it with some files
                File.WriteAllBytes(testPackage1Path, testPackage1Bytes);
                File.WriteAllBytes(testPackage2Path, testPackage2Bytes);
                File.WriteAllBytes(testTs4ScriptPath, testTs4ScriptBytes);
                File.WriteAllText(testReadmePath, testReadmeText);
            }
            
            [TearDown]
            public void TearDown()
            {
                if (Directory.Exists(testModDirPath))
                {
                    Directory.Delete(testModDirPath, true);
                }

                ServiceLocator.Reset();
            }
            
            [Test]
            public void AddMod_CopyModTest()
            {
                //quick check that the test setup is it self correct
                Assert.That(testModDirInfo.Exists, Is.True,
                    "Test mod directory should exist");

                //create the lib
                var lib = new JsonMonolithModLibraryService();
                ServiceLocator.Register<IModLibraryService>(lib);
                 
                Assert.That(lib.ActiveRig, Is.Not.Null,
                    "ActiveRig should not be null after " +
                    "library registration.");

                Assert.That(lib, Is.Not.Null, 
                    "Library should be created successfully");

                //add a test mod to it
                var testModEntry = new ModEntry(testModDirPath, null);
                lib.TryAddMod(testModEntry);

                Assert.That(lib.ModList, Has.Count.EqualTo(1), 
                    "Library count should reflect the added mod");

                Debug.WriteLine("Mod 0: " + lib.ModList[0]);

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
                //quick check that the test setup is it self correct
                Assert.That(testModDirInfo.Exists, Is.True,
                    "Test mod directory should exist");

                //create the lib
                var lib = new JsonMonolithModLibraryService();
                ServiceLocator.Register<IModLibraryService>(lib);
                
                Assert.That(lib.ActiveRig, Is.Not.Null,
                    "ActiveRig should not be null " +
                    "after library registration.");

                Assert.That(lib, Is.Not.Null, 
                    "Library should be created successfully");

                //add a test mod to it
                var testModEntry = new ModEntry(testModDirPath, null);

                var slug = testModEntry.Slug;
                Assert.That(slug, Is.Not.Null, 
                    "Test Check - Mod should have a slug");
                Assert.That(slug.ToString(), Is.Not.Empty, 
                    "Test Check - Mod slug should not be empty");
                
                var expectedNull = lib.GetModEntry(slug);
                Assert.That(expectedNull, Is.Null, 
                    "Test Check - Mod should not be in library" +
                    " yet during this part of the test.");
                try
                {
                    lib.ActiveRig?.TryAddModEntryToEnd(testModEntry);
                    Assert.Fail(
                        "Should not be able to add a mod to a rig " +
                        "that isn't in the library");
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine(
                        $"Got expected exception when attempting to " +
                        $"add a non-library mod to a rig: {ex}");
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"Unexpected exception when attempting to " +
                        $"add a non-library mod to a rig: {ex}");
                }
                
                lib.TryAddMod(testModEntry);
                Assert.That(lib.ModList, Has.Count.EqualTo(1), 
                    "Library count should reflect the added mod");
                Debug.WriteLine("Mod 0: " + lib.ModList[0]);
                
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
    
    public class RigTests : AbstractPlumbobTest
    {
        [Test]
        public void GetVisibleMods_CountTest()
        {
            //create the lib
            var lib = new JsonMonolithModLibraryService();
            lib.OnRegister(lib.GetType());
            Assert.That(lib.ActiveRig, Is.Not.Null, 
                "ActiveRig should not be null after adding a mod");

            Assert.That(lib.GetVisibleMods, Is.Not.Null, "Visible mods collection should be initialized");
            Assert.That(lib.GetVisibleMods().Count, Is.EqualTo(0), "Visible mods collection should be empty initially");

            var newEntry = ModEntry.CreateNewUnique("testpath"); 
            lib.TryAddMod(newEntry);
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