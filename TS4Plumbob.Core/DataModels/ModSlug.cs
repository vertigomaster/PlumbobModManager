namespace TS4Plumbob.Core.DataModels;

public record ModSlug
{
    /// <summary>
    /// The main id of the mod. Generally sourced from the mod's name.
    /// </summary>
    public string MainId { get; init; }

    /// <summary>
    /// Collisions are resolved by appending a number between the id and the version code.
    /// </summary>
    public int Offset { get; init; }

    public ModSlug()
    {
        MainId = string.Empty;
        Offset = 0;
    }
    
    public ModSlug(string mainId, int offset)
    {
        MainId = SanitizeForSlug(mainId);
        Offset = offset;
    }

    public ModSlug BumpCopy()
    {
        return new ModSlug(MainId, Offset + 1);
    }

    /// <summary>
    /// This will prepare a given string to be used as a slug id.
    /// The actual slug is made by composing the sanitized id with the version.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string SanitizeForSlug(string str)
    {
        //TS4 has a much easier time parsing underscores. 
        return System.Text.RegularExpressions.Regex
            .Replace(str, @"[.\-\s\t\r\n\u00A0\u2000-\u200B\u202F\u205F\u3000]", "_").ToLowerInvariant();
    }

    public static implicit operator string(ModSlug modSlug) => modSlug.ToString();

    public override string ToString() => $"{MainId}{(Offset > 0 ? $"_{Offset}" : "")}";
}