using System.Text.Json.Serialization;

namespace TS4Plumbob.Core.DataModels;

public record ModMetadata(
    string Name, 
    string Version, 
    AuthorProfile Author,
    
    //not sure if we actually need to specify 
    [property: JsonPropertyName("links")] 
    Dictionary<string,string> Links, 
    
    DateTime? LastUpdated = null
);