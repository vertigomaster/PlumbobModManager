using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The application title, resolved from service locator.
    /// </summary>
    public string AppTitle => ServiceLocator.Resolve<AppConfig>()?.FullAppName ?? "Plumbob Mod Manager";

    /// <summary>
    /// A friendly greeting message for the user.
    /// </summary>
    public string Greeting => $"Welcome to {AppTitle}!";

    /// <summary>
    /// The currently selected mod library folder. 
    /// The [ObservableProperty] attribute automatically generates a 'ModLibraryFolder' property 
    /// that notifies the UI when its value changes.
    /// </summary>
    [ObservableProperty] 
    private IStorageFolder? _modLibraryFolder;

    /// <summary>
    /// An interaction that allows the ViewModel to request the UI to show a folder picker.
    /// In this case, it takes no input (object?) and returns an IStorageFolder? result.
    /// </summary>
    public AsyncInteraction<object?, IStorageFolder?> OpenFolderPicker { get; } = new();

    /// <summary>
    /// Command that initiates the process of picking a folder.
    /// The [RelayCommand] attribute makes this method accessible as a command from XAML bindings.
    /// </summary>
    [RelayCommand]
    private async Task UpdateModLibraryFolder()
    {
        // Trigger the interaction and wait for the UI to provide a result (or null if cancelled).
        var result = await OpenFolderPicker.Handle(null);
        if (result != null)
        {
            // Update our observable property with the selected folder.
            ModLibraryFolder = result;
        }
    }
}