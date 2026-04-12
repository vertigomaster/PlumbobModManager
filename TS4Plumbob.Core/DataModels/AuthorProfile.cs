namespace TS4Plumbob.Core.DataModels;
public record AuthorProfile(
    Guid Id,
    string Name,
    string[] NewsUrls,
    string? MainModSiteUrl = null
);
