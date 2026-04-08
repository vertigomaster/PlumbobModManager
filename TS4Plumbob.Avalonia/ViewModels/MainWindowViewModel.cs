using Avalonia.Platform.Storage;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string AppTitle => ServiceLocator.Resolve<AppConfig>()?.FullAppName ?? "Plumbob Mod Manager";
    public string Greeting => $"Welcome to {AppTitle}!";
    public IStorageFolder ModLibraryFolder { get; set; }
}