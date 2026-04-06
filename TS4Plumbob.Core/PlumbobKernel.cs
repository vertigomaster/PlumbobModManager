using System.Diagnostics;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopUtils.Services;
using IDEK.Tools.Trove;
using IDEK.Tools.Utilities;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Core;

public partial class PlumbobKernel()
{
    [Flags]
    public enum LogMode
    {
        None = 0, //no v
        Console, //should the kernel output logs to console (stdout, stderr, etc?)
        File, //should the Kernel output logs to an explicit file?
        LogsToScreen, //should logs pop up a dialog? (not recommended)
        WarningsToScreen, //should warnings pop up a dialog?
        ErrorsToScreen, //should errors pop up a dialog?
    }
    
    internal Trove coreLifetimeTrove;
    public IdekEvent BindExternalServicesEvent = new();

    public LogMode LoggingMode { get; set; } = LogMode.Console;

    public bool IsRunning { get; private set; } = false;
    
    /// <summary>
    /// Boots up the core, binding the services it provides and enabling various events and logic.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<int> Start(string[] args)
    {
        ConsoleLog.Log("Attempting to start PMM Kernel...");
        if (IsRunning) return 0;
        ConsoleLog.Log("Starting PMM Kernel...");
        
        IsRunning = true;
        
        coreLifetimeTrove = new Trove("Plumbob Core Kernel");
        
        //here for now, may need to change or move to the application projects that use this library.
        BindCoreServices();
        BindExternalServicesEvent?.Invoke();
        
        return 0;
    }

    public async Task<int> Shutdown()
    {
        ConsoleLog.Log("Attempting shutdown PMM Kernel...");
        if (!IsRunning) return 0;
        ConsoleLog.Log("Shutting down PMM Kernel...");
        
        DisposeCoreServices();
        
        IsRunning = false;
        return 0;
    }

    /// <summary>
    /// bind services (might do a partial if this gets really big)
    /// </summary>
    private void BindCoreServices()
    {
        ConsoleLog.Log("Binding Core Services...");
        ServiceLocator.BindJumpstarter<AppConfig>(() =>
        {
            ConsoleLog.Log("Jumpstarting AppConfig...");
            AppConfig newService = AppConfig.LoadFromDisk() ?? new AppConfig();

            coreLifetimeTrove.AddCleanup(nameof(AppConfig), () =>
            {
                ServiceLocator.TryUnregister<AppConfig>(newService);
                newService.SaveToDisk();
            });

            return newService;
        });
        
        ServiceLocator.BindJumpstarter<IModLibraryService>(() => {
            ConsoleLog.Log("Jumpstarting IModLibraryService via JsonMonolithModLibraryService...");
            string location = ServiceLocator.Resolve<AppConfig>()
                .UserSettings.ModLibraryPath;
            
            var newService = JsonMonolithModLibraryService.LoadFromFile(location) ?? 
                new JsonMonolithModLibraryService();

            coreLifetimeTrove.AddCleanup(nameof(JsonMonolithModLibraryService), () => {
                ServiceLocator.TryUnregister<IModLibraryService>(newService);
                newService.SaveToFile(location);
            });
            
            return newService;
        });
    }

    private void DisposeCoreServices()
    {
        ConsoleLog.Log("Disposing Core Services...");
        coreLifetimeTrove.Dispose(); //well that was easy
    }
}