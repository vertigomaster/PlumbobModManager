namespace TS4Plumbob.Core.DataModels;

/// <summary>
/// Unique Human-readable identifier 
/// </summary>
public record ModEntrySlug : ModSlug
{
    /// <summary>
    /// The version of the mod.
    /// </summary>
    public Version Version { get; init; }

    /// <summary>
    /// Special and optional case for mods which have a specific
    /// or non-numeric version component,
    /// like a/b for alpha/beta or LE vs standard
    /// </summary>
    public string VariantString { get; init; }

    public ModEntrySlug()
    {
        Version = Version.Parse("0.0.0");
        VariantString = "";
    }

    public ModEntrySlug(string mainId, int offset, string variant = "") : base(mainId, offset)
    {
        Version = Version.Parse("0.0.0");
        VariantString = variant;
    }
    
    public ModEntrySlug(string mainId, Version version, int offset, string variant = "") : base(mainId, offset)
    {
        Version = version;
        VariantString = variant;
    }
    
    public ModEntrySlug(ModSlug modSlug, Version version, string variant = "") : base(modSlug.MainId, modSlug.Offset)
    {
        Version = version;
        VariantString = variant;
    }

    public ModEntrySlug(string mainId, Version version, string variant="") : base(mainId, 0)
    {
        Version = version;
        VariantString = variant;
    }
    
    public ModEntrySlug(string mainId, string version, int offset, string variant = "") : base(mainId, offset)
    {
        Version = Version.Parse(version);
        VariantString = variant;   
    }
    
    public static implicit operator string(ModEntrySlug modEntrySlug) => modEntrySlug.ToString();
    
    public override string ToString() => $"{MainId}{(Offset > 0 ? $"_{Offset}" : "")}-{SanitizeForSlug(Version.ToString())}{VariantString}";
}