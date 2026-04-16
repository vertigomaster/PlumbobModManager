using System.Diagnostics;

namespace PlumbobModManager.Tests;

[TestFixture]
public abstract class AbstractPlumbobTest
{
    protected const string BASE_DIR = TestUtils.PMM_UNIT_TEST_BASE_DIR;
    
    [SetUp]
    public void SetupTestDirectory()
    {
        Debug.WriteLine("[PlumbobTest Setup] Setting up test directory");
        
        //reset with a fresh directory for testing
        if (Directory.Exists(BASE_DIR))
        {
            Directory.Delete(BASE_DIR, true);
        }

        Directory.CreateDirectory(BASE_DIR);
        Debug.WriteLine("[PlumbobTest Setup] Done setting up test directory");
    }

    [TearDown]
    public void TearDownTestDirectory()
    {
        Debug.WriteLine("[PlumbobTest TearDown] Cleaning up test directory");
        if (Directory.Exists(BASE_DIR))
        {
            Directory.Delete(BASE_DIR, true);
        }
        Debug.WriteLine("[PlumbobTest TearDown] Done cleaning up test directory");
    }
}