using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDEK.Tools.ShocktroopUtils.Services;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia.ViewModels;

public partial class RootViewModel : ViewModelBase
{
    //Properties are data that Views can read/bind to. VM reacts to what the View directly reports to it.
    //VM can also interact with the model (or rather passively react to the Model so that it can run state update operations that ultimately update properties, which in turn update the View)
    #region Properties

    /// <summary>
    /// The application title, resolved from service locator.
    /// </summary>
    public string AppTitle => ServiceLocator.Resolve<AppConfig>()?.FullAppName ?? "Plumbob Mod Manager";
    
    /// <summary>
    /// The currently selected mod library folder. 
    /// The [ObservableProperty] attribute automatically generates a 'ModLibraryFolder' property 
    /// that notifies the UI when its value changes.
    /// </summary>
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasLibraryFolder))]
    private IStorageFolder? _modLibraryFolder;
    public bool HasLibraryFolder => ModLibraryFolder != null;

    #endregion

    //Interactions let the VM trigger logic that is dependent on stuff from the view.
    //In this case, it's mostly TopLevel that we need
    #region Interactions

    /// <summary>
    /// 
    /// </summary>
    public AsyncInteraction<string?, IStorageFolder?> OpenFolderPicker { get; } = new();
    

    #endregion

    //Actions the View can instruct the VM to take, which are triggered by View command bindings.
    //Mainly from buttons and such.
    #region Commands

    /// <summary>
    /// Command that initiates the process of picking a folder.
    /// it uses the OpenFolderPicker interaction to modify the ModLibraryFolder state property.
    /// </summary>
    /// <remarks>
    /// Anyone should be able to call this;
    /// it'll generally be done from either the options menu view or from the NoDataView
    /// </remarks>
    [RelayCommand]
    private async Task UpdateModLibraryFolder()
    {
        // Trigger the interaction and wait for the UI to provide a result (or null if cancelled).
        IStorageFolder? result = await OpenFolderPicker.Handle("Select Mod Library Folder");

        // Update our observable property with the selected folder.
        ModLibraryFolder = result;
    }
    
    #endregion
    
}