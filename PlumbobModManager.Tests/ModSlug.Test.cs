using TS4Plumbob.Core.DataModels;

namespace PlumbobModManager.Tests;

public class ModSlug_Test : AbstractPlumbobTest
{
    [Test]
    public void SanitizeForSlug_BasicSanitation()
    {
        string input = "This is a test string with invalid characters";
        string expectedOutput = "this_is_a_test_string_with_invalid_characters";
        string actualOutput = ModSlug.SanitizeForSlug(input);

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Spaces should be removed during slugification and everything should become lowercase!\n" +
            $" got: '{actualOutput}' but expected: '{expectedOutput}'");
    }
    [Test]
    public void SanitizeForSlug_RemovalOfRiskySymbolChars()
    {
        string input = @"This is a test string with invalid characters: <>:""/\\|?*!@#$%^&:()";
        //20, not 21; first of the \\ sequence is an escape character.
        string expectedOutput = "this_is_a_test_string_with_invalid_characters____________________()";
        string actualOutput = ModSlug.SanitizeForSlug(input);

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Invalid or risky symbol characters should be removed during slugification. Input was: \"{input}\"");
    }

    [Test]
    public void BumpCopy_IncrementsOffset()
    {
        var initialSlug = new ModSlug("test_slug", 0);
        string expectedBumpToString = $"{initialSlug.MainId}_1";
        
        var bumpedSlug = initialSlug.BumpCopy();
        Assert.That(bumpedSlug.Offset, Is.EqualTo(1), "Offset should be incremented by 1 by default");
        Assert.That(bumpedSlug.MainId, Is.EqualTo(initialSlug.MainId), "MainId should remain the same after bumping");
        Assert.That(bumpedSlug, Is.Not.EqualTo(initialSlug), "Slugs are records, so two slugs with different offsets should not be considered equal, even if their MainIds are the same.");
        Assert.That(bumpedSlug.ToString(), Is.EqualTo($"{initialSlug.MainId}_{bumpedSlug.Offset}"), "Slug ToString() should match the expected format");
        
        var bumpedSlug2 = bumpedSlug.BumpCopy(2);
        Assert.That(bumpedSlug2.Offset, Is.EqualTo(3), "Offset of the 1-bumped slug should become 3 after being bumped by 2 specifically (1 + 2 = 3)");
        Assert.That(bumpedSlug2.MainId, Is.EqualTo(initialSlug.MainId), "MainId should still remain the same after multiple strung bumps");
        
        int bigBump = 1234567890;
        var bigBumpedSlug = bumpedSlug.BumpCopy(bigBump);
        Assert.That(bigBumpedSlug.Offset, Is.EqualTo(bumpedSlug.Offset + bigBump), "Offset should be incremented by the specified amount (unless integer overflows)");
        
        var bumpedSlug3 = initialSlug.BumpCopy(1);
        Assert.That(bumpedSlug3.Offset, Is.EqualTo(bumpedSlug.Offset), "Bumping the same slug record by 1 two times should produce two slugs with the same offset");
        Assert.That(bumpedSlug3.MainId, Is.EqualTo(bumpedSlug.MainId), "Bumping the same slug record by two times should not change the MainId");
        Assert.That(bumpedSlug3, Is.EqualTo(bumpedSlug), "Slugs are records, so two slugs with the same MainId and offset should be considered equal, even if bumped.");
    }
    
    [Test]
    public void BumpCopy_ToString()
    {
        var initialSlug = new ModSlug("test_slug", 0);
        Assert.That(initialSlug.ToString(), Is.EqualTo($"{initialSlug.MainId}"), "Slug ToString() should only show offset if it is greater than 0");
        
        var bumpedSlug = initialSlug.BumpCopy(1);
        Assert.That(bumpedSlug.ToString(), Is.EqualTo($"{initialSlug.MainId}_1"), "Slug ToString() should match the expected format");

        int testNumber = 1234567890;
        var bumpedSlug2 = initialSlug.BumpCopy(testNumber);
        Assert.That(bumpedSlug2.ToString(), Is.EqualTo($"{initialSlug.MainId}_{testNumber}"),
            "Slug ToString() should match the expected format");
    }
    
    [Test]
    public void BumpCopy_DisallowNegativeOffset()
    {
        var initialSlug = new ModSlug("test_slug", 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => initialSlug.BumpCopy(-1), 
            "Offset should not be allowed to be negative");
    }
    
    [Test]
    public void BumpCopy_DisallowZeroOffset()
    {
        var initialSlug = new ModSlug("test_slug", 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => initialSlug.BumpCopy(0), 
            "Bump by 0 should not be allowed - that does not actually bump the slug and can lead to duplicate slugs");
    }

    [Test]
    public void Unknown_CorrectlyConstructProperty()
    {
        Assert.That(ModSlug.Unknown, Is.Not.Null, "ModSlug.Unknown property should not evaluate to null");
        Assert.That(ModSlug.Unknown.MainId, Is.EqualTo("unknown"), "MainId should be 'unknown'");
        Assert.That(ModSlug.Unknown.Offset, Is.EqualTo(0), "Offset should be 0");
    }
}