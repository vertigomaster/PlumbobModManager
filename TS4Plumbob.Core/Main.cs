using IDEK.Tools.ShocktroopUtils.Services;
using IDEK.Tools.Trove;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Core;

public partial class Main
{
    internal Trove coreLifetimeTrove;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<int> Start(string[] args)
    {
        coreLifetimeTrove = new Trove();
        
        //here for now, may need to change or move to the application projects that use this library.
        BindCoreServices();
        
        return 0;
    }

    public async Task<int> Shutdown()
    {
        DisposeCoreServices();
        return 0;
    }

    /// <summary>
    /// bind services (might do a partial if this gets really big)
    /// </summary>
    private void BindCoreServices()
    {
        ServiceLocator.BindJumpstarter<AppConfig>(() =>
        {
            AppConfig newService = AppConfig.LoadFromDisk() ?? new AppConfig();
            
            coreLifetimeTrove.AddCleanup(nameof(AppConfig), () => 
                ServiceLocator.TryUnregister<AppConfig>(newService));
            
            return newService;
        });
        
        ServiceLocator.BindJumpstarter<IModLibraryService>(() => {
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
        coreLifetimeTrove.Dispose(); //well that was easy
    }
}