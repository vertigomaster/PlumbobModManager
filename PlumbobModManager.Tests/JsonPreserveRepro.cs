using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace PlumbobModManager.Tests;

public record SimpleModEntry(string Name);

public class SimpleLib
{
    [JsonInclude]
    public List<SimpleModEntry> Mods = new();
}

public class JsonPreserveReproTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    [Test]
    public void Repro_ReferenceNotFound()
    {
        var lib = new SimpleLib();
        lib.Mods.Add(new SimpleModEntry("Test"));

        string json = JsonSerializer.Serialize(lib, Options);
        Console.WriteLine("[DEBUG_LOG] JSON: " + json);

        var deserialized = JsonSerializer.Deserialize<SimpleLib>(json, Options);
        Assert.That(deserialized.Mods.Count, Is.EqualTo(1));
    }
}
