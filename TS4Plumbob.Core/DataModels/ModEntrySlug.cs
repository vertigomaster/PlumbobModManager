namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// Unique Human-readable identifier 
/// </summary>
public record ModEntrySlug : ModSlug
{
    /// <summary>
    /// The version of the mod.
    /// </summary>
    public string Version { get; init; }

    public ModEntrySlug()
    {
        Version = string.Empty;
    }
        
    public ModEntrySlug(string mainId, string version, int offset) : base(mainId, offset)
    {
        Version = SanitizeForSlug(version);
    }
    
    public static implicit operator string(ModEntrySlug modEntrySlug) => modEntrySlug.ToString();
    
    public override string ToString() => $"{MainId}{(Offset > 0 ? $"_{Offset}" : "")}-{Version}";
}