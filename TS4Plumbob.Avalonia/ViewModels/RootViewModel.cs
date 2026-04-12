using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDEK.Tools.ShocktroopUtils.Services;
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
    
    /// <summary>
    /// The currently selected mod library folder. 
    /// The [ObservableProperty] attribute automatically generates a 'ModLibraryFolder' property 
    /// that notifies the UI when its value changes.
    /// </summary>
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasLibraryFolder))]
    private IStorageFolder? _modLibraryFolder;
    public bool HasLibraryFolder => ModLibraryFolder != null;
    
    //This contains an editable list of all mods that should be visible based on application and model state. Generally that means it is impacted by the current rig/profile and any filters.
    public ObservableCollection<ModEntryViewModel> VisibleMods { get; set; }

    #endregion

    public RootViewModel()
    {
        //Initialize properties
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
        var visibleMods = Library.GetVisibleMods().Select(m => {
            var entry = Library.GetModEntry(m.ModId);
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
            Console.WriteLine("[UpdateModLibraryFolder] " +
                "Something went wrong. " +
                "User selection was likely invalid.");
            return;
        }

        if (pickerTask.IsCanceled)
        {
            Console.WriteLine("[UpdateModLibraryFolder] " +
                "User cancelled folder selection.");
            return;
        }
        
        if(result is null)
        {
            Console.WriteLine("[UpdateModLibraryFolder] Something went wrong. Picker task was successful but result was null.");
            return;
        }
        
        if (pickerTask.IsCompletedSuccessfully)
        {
            Console.WriteLine("[UpdateModLibraryFolder] User selected folder: " + result.Path.LocalPath);
            // Update our observable property with the selected folder.
            ModLibraryFolder = result;

            //update model
            Config.UserSettings.ModLibraryPath = ModLibraryFolder?.Path.LocalPath;
            Config.SaveToDisk();
        }
    }

    [RelayCommand]
    private async Task CopyModToLibrary()
    {
        IStorageFolder? result = await OpenFolderPicker.Handle("Select Mod to Copy into Library");
        if (result is null) return;

        Library.TryAddMod(ModEntry.CreateNewUnique(result.Path.LocalPath, null));
    }
    
    #endregion
    
}