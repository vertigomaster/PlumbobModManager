using System.Runtime.CompilerServices;
using IDEK.Tools.ShocktroopUtils.Services;
using IDEK.Tools.Trove;
using IDEK.Tools.Utilities;
using Plumbob.Core.Utils;
using TS4Plumbob.Core.DataModels;

[assembly: InternalsVisibleTo("PlumbobModManager.Tests")]

namespace TS4Plumbob.Core;

public partial class PlumbobKernel()
{
    public static PlumbobKernel Instance { get; private set; } = new();
    
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

    public static async Task<int> StartInstance(string[] args)
    {
        PlumbobMsg.WriteDebugInfo("Attempting to start PMM Kernel Instance...");
        if (Instance.IsRunning) return 0;
        PlumbobMsg.WriteDebugInfo("Starting PMM Kernel Instance...");

        Instance = new PlumbobKernel();
        return await Instance.Start(args);
    }
    
    /// <summary>
    /// Boots up the core, binding the services it provides and enabling various events and logic.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<int> Start(string[] args)
    {
        PlumbobMsg.WriteDebugInfo("Checking if PMM Kernel already running...");
        if (IsRunning) return 0;
        PlumbobMsg.WriteDebugInfo("Kernel not already running, Starting PMM Kernel...");
        
        IsRunning = true;
        
        coreLifetimeTrove = new Trove("Plumbob Core Kernel");
        
        //here for now, may need to change or move to the application projects that use this library.
        BindCoreServices();
        BindExternalServicesEvent?.Invoke();
        
        return 0;
    }

    public async Task<int> Shutdown()
    {
        PlumbobMsg.WriteDebugInfo("Attempting shutdown PMM Kernel...");
        if (!IsRunning) return 0;
        PlumbobMsg.WriteDebugInfo("Shutting down PMM Kernel...");
        
        DisposeCoreServices();
        
        IsRunning = false;
        return 0;
    }

    /// <summary>
    /// bind services (might do a partial if this gets really big)
    /// </summary>
    private void BindCoreServices()
    {
        PlumbobMsg.WriteDebugInfo("Binding Core Services...");
        ServiceLocator.BindJumpstarter<AppConfig>(() => {
            AppConfig newService = AppConfig.LoadFromDisk() ?? new AppConfig();

            coreLifetimeTrove.AddCleanup(nameof(AppConfig), () => {
                PlumbobMsg.WriteDebugInfo("Cleaning up AppConfig Service...");
                ServiceLocator.TryUnregister<AppConfig>(newService);
                newService.SaveToDisk();
                PlumbobMsg.WriteDebugInfo("AppConfig Service Cleanup Complete.");
            });

            return newService;
        });
        
        ServiceLocator.BindJumpstarter<IModLibraryService>(() => {
            
            
            var newService = JsonMonolithModLibraryService.LoadFromFile() ?? 
                new JsonMonolithModLibraryService();

            coreLifetimeTrove.AddCleanup(nameof(JsonMonolithModLibraryService), () => {
                PlumbobMsg.WriteDebugInfo("Cleaning up ModLibraryService...");
                ServiceLocator.TryUnregister<IModLibraryService>(newService);
                newService.SaveToFile();
                PlumbobMsg.WriteDebugInfo("ModLibraryService Cleanup Complete.");
            });

            return newService;
        });
    }

    private void DisposeCoreServices()
    {
        PlumbobMsg.WriteDebugInfo("Disposing Core Services...");
        coreLifetimeTrove.Dispose(); //well that was easy
        PlumbobMsg.WriteDebugInfo("Core Services Disposed.");
    }
}