using TS4Plumbob.Core.DataModels.IdTypes;

namespace TS4Plumbob.Core.DataModels;
public record AuthorProfile(
    AuthorId Id,
    string Name,
    string[] NewsUrls,
    string? MainModSiteUrl = null
);
