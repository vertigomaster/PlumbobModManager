using System.Text.Json.Serialization;

namespace TS4Plumbob.Core.DataModels;

public record ModMetadata(
    string Name,
    Version Version,
    AuthorProfile Author,
    DateTime? LastUpdated = null
)
{
    public string VariantString { get; set; } = "";
    public static ModMetadata Unknown => new(
        "[Unknown]",
        Version.Parse("0.0.0"),
        AuthorProfile.Unknown,
        DateTime.Now
    );
}