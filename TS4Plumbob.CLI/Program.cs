using TS4PlumbobCore.DataModels;

namespace PlumbobMMCLI;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! It's Plumbob Mod Manager time! Woo!");

        AuthorProfile profile = new AuthorProfile("Author Mr. Vertigo", new []{"bob.com"}, "https://github.com/vertigomaster");
        
        Console.WriteLine($"Profile Name: {profile.Name}, at {profile.MainModSiteUrl}");
    }
}