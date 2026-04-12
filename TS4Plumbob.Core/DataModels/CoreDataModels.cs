//should be fine for these simpler models; Newtonsoft is a good alternative though.

using System.Text.Json.Serialization;

namespace TS4Plumbob.Core.DataModels;

public record AuthorProfile(
    Guid Id,
    string Name,
    string[] NewsUrls,
    string? MainModSiteUrl = null
);

public record ModRig(
    Guid Id,
    string Name,
    string Description,
    ModManifestSnapshot Manifest
);

public record ModMetadata(
    string Name, 
    string Version, 
    AuthorProfile Author,
    
    //not sure if we actually need to specify 
    [property: JsonPropertyName("links")] 
    Dictionary<string,string> Links, 
    
    DateTime? LastUpdated = null
);

public record ModEntry(
    Guid Id,
    string RootPath,
    bool HasMetadata,
    ModMetadata? ModMetadata)
{
    public bool ExistsOnDisk() 
    {
        return Directory.Exists(RootPath);
    }
}