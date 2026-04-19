namespace TS4Plumbob.Core.DataModels;
public record AuthorProfile
{
    public string Name { get; init; }
    public string[] NewsUrls { get; init; }
    public string? MainModSiteUrl { get; init; }

    //Reminder that in order to support (de)serialization with System.Text.Json,
    //a default parameterless constructor is required so that the serializer
    //can properly instantiate the class and populate the properties.
    public AuthorProfile() { }

    public AuthorProfile(string name, string[] newsUrls, string? mainModSiteUrl = null)
    {
        Name = name;
        NewsUrls = newsUrls;
        MainModSiteUrl = mainModSiteUrl;
    }

    //record value equality has its limits; if its members are themselves classes,
    //those MEMBERS will still have their equality compared by REFERENCE,
    //by default at least, as the record equality simply goes down asking
    //each record to resolve its own eqaulity. with the opposing record's
    //respective member.
    //Thusly, equality must still be manually defined/overriden for records which contain
    //class members that need to be compared by something other than reference.
    public virtual bool Equals(AuthorProfile? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && 
               NewsUrls.SequenceEqual(other.NewsUrls) && 
               MainModSiteUrl == other.MainModSiteUrl;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Name);
        foreach (var url in NewsUrls)
        {
            hashCode.Add(url);
        }
        hashCode.Add(MainModSiteUrl);
        return hashCode.ToHashCode();
    }

    public static AuthorProfile Unknown => new("Unknown", [], null);
}
