using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TS4Plumbob.Avalonia.ViewModels;

namespace TS4Plumbob.Avalonia.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Typed access to the ViewModel (cast from DataContext).
    /// </summary>
    public MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Called when the DataContext is updated. We use this to connect our
    /// UI-specific interactions to the ViewModel.
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null)
        {
            // Connect the ViewModel's request (OpenFolderPicker) to our UI implementation.
            ViewModel.OpenFolderPicker.RegisterHandler(_ => OpenFolderPicker());
        }
    }

    /// <summary>
    /// Cleanup logic when the Window is removed from the visual tree.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        // Unregister handlers to avoid memory leaks.
        ViewModel?.OpenFolderPicker.UnregisterHandler();
    }

    /// <summary>
    /// Platform-specific implementation of the folder picking logic.
    /// This uses the Avalonia StorageProvider API.
    /// </summary>
    /// <returns>The selected IStorageFolder, or null if cancelled.</returns>
    public async Task<IStorageFolder?> OpenFolderPicker()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;
        
        var sp = topLevel.StorageProvider;
        
        // Try to start the picker in the user's Documents folder.
        Task<IStorageFolder?> getStartLocTask = 
            sp.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        
        IReadOnlyList<IStorageFolder> folders = await sp.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
        {
            Title = "Select Mod Library Folder",
            SuggestedStartLocation = await getStartLocTask,
            AllowMultiple = false
        });

        return folders.Count >= 1 ? folders[0] : null;
    }
}