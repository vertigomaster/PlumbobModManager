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
        string input = "This is a test string with invalid characters: !@#$%^&*()";
        string expectedOutput = "this_is_a_test_string_with_invalid_characters";
        string actualOutput = ModSlug.SanitizeForSlug(input);

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Invalid or risky symbol characters should be removed during slugification\n" +
            $" got: '{actualOutput}' but expected: '{expectedOutput}'");
    }
}