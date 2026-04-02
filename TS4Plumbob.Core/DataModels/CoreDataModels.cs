//should be fine for these simpler models; Newtonsoft is a good alternative though.
using System.Text.Json.Serialization; 
namespace TS4PlumbobCore.DataModels;

public record AuthorProfile(
    string Name,
    string[] NewsUrls,
    string? MainModSiteUrl = null
);
    
public record ModMetadata(
    string Name, 
    string Version, 
    AuthorProfile Author,
    
    [property: JsonPropertyName("links")] //sure if we actually need to specify 
    Dictionary<string,string> Links, 
    
    DateTime? LastUpdated = null
);

public record ModEntry(
    string RootPath,
    bool HasMetadata,
    ModMetadata? ModMetadata
);