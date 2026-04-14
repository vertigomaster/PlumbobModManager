using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Services;
using Plumbob.Core.Utils;
using TS4Plumbob.Core;
using TS4Plumbob.Core.DataModels;

namespace TS4Plumbob.Avalonia.ViewModels;

public class ModEntryViewModel : ViewModelBase
{
    public string ModName { get; set; }
    public string ModVersion { get; set; }
    public string ModAuthor { get; set; }
    public string ModDescription { get; set; }
    public bool IsEnabled { get; set; }
}

public partial class RootViewModel : ViewModelBase
{
    //these properties aren't really necessary, but they're convenient for key services.
    
    //Properties are data that Views can read/bind to. VM reacts to what the View directly reports to it.
    //VM can also interact with the model (or rather passively react to the Model so that it can run state update operations that ultimately update properties, which in turn update the View)
    #region Properties

    /// <summary>
    /// The application title, resolved from service locator.
    /// </summary>
    public string AppTitle => Config?.FullAppName ?? "Plumbob Mod Manager";
    
    // [ObservableProperty] 
    // [NotifyPropertyChangedFor(nameof(HasLibraryFolder))]
    // private IStorageFolder? _modLibraryFolder;
    
    /// <summary>
    /// The currently selected mod library folder. 
    /// The [ObservableProperty] attribute automatically generates a 'ModLibraryFolder' property 
    /// that notifies the UI when its value changes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLibraryFolder))]
    private string _modLibraryFolderString;
    
    public bool HasLibraryFolder => !ModLibraryFolderString.IsNullOrWhitespace();
    
    //This contains an editable list of all mods that should be visible based on application and model state. Generally that means it is impacted by the current rig/profile and any filters.
    public ObservableCollection<ModEntryViewModel> VisibleMods { get; set; }

    #endregion

    public RootViewModel()
    {
        //Initialize properties
        ModLibraryFolderString = Config.UserSettings.ModLibraryPath;
        
        PlumbobMsg.WriteDebugInfo("RootViewModel initialized with ModLibraryFolderString: " + ModLibraryFolderString);
    }

    #region Property Change Callbacks

    // protected override void OnModLibraryFolderChanged()
    // {
    //     
    // }
    
    #endregion
    
    //The VM can respond to changes in the model by subscribing to events.
    //Those events are then handled by the VM here.
    #region Model Callbacks
    
    //TODO: react to the model changing what mods are visible
    
    private void OnVisibleModsRefreshed()
    {
        var visibleMods = Library.GetVisibleMods().Select(entry => {
            return new ModEntryViewModel {
                ModName = entry?.HumanReadableIdentifier ?? "Unknown",
                ModVersion = entry?.ModMetadata?.Version ?? "0.0.0",
                ModAuthor = entry?.ModMetadata?.Author?.Name ?? "Unknown",
                ModDescription = "", // TODO: Add description to ModMetadata if needed
                IsEnabled = true // TODO: Logic for enabled state
            };
        });
        VisibleMods = new ObservableCollection<ModEntryViewModel>(visibleMods);
    }
    
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
        var pickerTask = OpenFolderPicker.Handle("Select Mod Library Folder");
        
        IStorageFolder? result = await pickerTask;

        if (pickerTask.IsFaulted)
        {
            //TODO: show error message
            PlumbobMsg.WriteUserMsg("Something went wrong. " +
                "User selection was likely invalid.");
            return;
        }

        if (pickerTask.IsCanceled)
        {
            PlumbobMsg.WriteUserMsg("Something went wrong. Picker TASK was cancelled, " +
                "which counterintuitively is not what happens when the user clicks the cancel button.");
            return;
        }
        
        if(result is null)
        {
            PlumbobMsg.WriteUserMsg("User cancelled folder selection.");
            
            return;
        }
        
        if (pickerTask.IsCompletedSuccessfully)
        {
            ModLibraryFolderString = result?.Path.LocalPath ?? "";
            
            PlumbobMsg.WriteUserMsg("User selected folder: " + ModLibraryFolderString);
            // Update our observable property with the selected folder.

            //update model
            Config.UserSettings.ModLibraryPath = ModLibraryFolderString;
            Config.SaveToDisk();
        }
    }

    [RelayCommand]
    private async Task CopyModToLibrary()
    {
        IStorageFolder? result = await OpenFolderPicker.Handle("Select Mod to Copy into Library");
        if (result is null) return;
        
        //TODO: popup new window asking about mod setup

        if (Library.TryAddMod(ModEntry.CreateNewUnique(result.Path.LocalPath, null)))
        {
            Library.SaveToFile();
        }
    }
    
    #endregion
    
}