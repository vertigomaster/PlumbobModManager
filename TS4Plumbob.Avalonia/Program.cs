using Avalonia;
using System;
using System.Threading.Tasks;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        //Try boot up the kernel beforehand
        int startCode = await PlumbobKernel.StartInstance(args);
        if (startCode != 0) return;

        //one big Bandaid so that I don't have to add a bunch of weird little
        //async bandaids all over the place for Library property access
        //forces the service locator to resolve this core async service
        await ServiceLocator.ResolveAsync<IModLibraryService>();
        //now the service should be resolved and present fpr synchronous access
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        
        Console.WriteLine("Log test - does this fire before or after avalonia?");
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => 
        AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}