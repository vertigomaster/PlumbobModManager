using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TS4Plumbob.Avalonia.ViewModels;

namespace TS4Plumbob.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
    
    public MainWindow()
    {
        InitializeComponent();
    }
    
    //for sake of testing
    public async Task<IStorageFolder> OpenFolderPicker()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;
        
        var sp = topLevel.StorageProvider;
        
        Task<IStorageFolder?> getStartLocTask = 
            sp.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        
        IReadOnlyList<IStorageFolder> folders = await sp.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
        {
            Title = "Select Mod Library Folder",
            SuggestedStartLocation = await getStartLocTask,
            AllowMultiple = false
        });

        if (folders.Count >= 1)
        {
            return folders[0];
        }
        else
        {
            return null;
        }
    }
    
    private async void OnUpdateModLibraryFolderClicked(object? sender, RoutedEventArgs e)
    {
        // Task.Run(() =>
        // {
        //
        // });
        IStorageFolder? folder = await OpenFolderPicker();
        ViewModel!.ModLibraryFolder = folder;
    }
    
    
}