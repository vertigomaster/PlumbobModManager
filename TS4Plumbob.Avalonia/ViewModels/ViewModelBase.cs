using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected static PlumbobKernel Core => PlumbobKernel.Instance;
    protected static AppConfig Config => ServiceLocator.Resolve<AppConfig>();
    
    protected static IModLibraryService Library => ServiceLocator.Resolve<IModLibraryService>();
}