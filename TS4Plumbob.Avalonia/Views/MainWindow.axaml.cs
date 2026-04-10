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
    public RootViewModel? MainViewModel => DataContext as RootViewModel;
    
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

        //together with the call in OnUnloaded, this enables the entire window to use a folder picker, as it requires access to a visual element, which the VM cannot directly touch.
        MainViewModel?.OpenFolderPicker.RegisterHandler(pickerTitle => PlumbobFileSystem.PickFolder(this, pickerTitle));
    }

    /// <summary>
    /// Cleanup logic when the Window is removed from the visual tree.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        MainViewModel?.OpenFolderPicker.UnregisterHandler();
    }

}