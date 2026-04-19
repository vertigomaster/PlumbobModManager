namespace TS4Plumbob.Core.DataModels;
public record AuthorProfile(
    string Name,
    string[] NewsUrls,
    string? MainModSiteUrl = null
)
{
    public static AuthorProfile Unknown => new("Unknown", [], null);
}
