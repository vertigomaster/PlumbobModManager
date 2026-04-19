using System.Text.Json.Serialization;

namespace TS4Plumbob.Core.DataModels;

public record ModMetadata
{
    public string Name { get; init; }
    public Version Version { get; init; }
    public AuthorProfile Author { get; init; }
    public DateTime? LastUpdated { get; init; }
    public string VariantString { get; set; } = "";

    public ModMetadata() { }

    public ModMetadata(string name, Version version, AuthorProfile author, DateTime? lastUpdated = null)
    {
        Name = name;
        Version = version;
        Author = author;
        LastUpdated = lastUpdated;
    }

    public static ModMetadata Unknown => new(
        "[Unknown]",
        Version.Parse("0.0.0"),
        AuthorProfile.Unknown,
        DateTime.Now
    );
}