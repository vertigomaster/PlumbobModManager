namespace TS4Plumbob.Core.DataModels;

public class Mod
{
    /// <summary>
    /// Represents the template that will be used as the basis for new entries.
    /// </summary>
    public ModEntry Template { get; set; }
    
    public ModSlug Slug { get; set; }
    
    public List<ModEntry> Entries { get; set; } = [];
    
    
}