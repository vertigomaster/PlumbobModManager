using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IDEK.Tools.Trove;
using TS4Plumbob.Avalonia.ViewModels;

namespace TS4Plumbob.Avalonia.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Typed access to the ViewModel (cast from DataContext).
    /// </summary>
    public RootViewModel? MainViewModel => DataContext as RootViewModel;
    
    private Trove _loadedTrove;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    #region Overrides of StyledElement

    //awake - called once, right after this view is created.
    //It is not yet displayed, but it has completed its basic initialization within Avalonia.
    //set up internal data and non-ui logic here
    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _loadedTrove = Trove.Create($"{typeof(MainWindow).FullName}-LoadedLifecycleTrove");
    }

    #endregion

    #region Overrides of TopLevel

    //start - called right before the view's axaml is evaluated and displayed
    //access references to the parts of your UI, as they are all loaded by this point but not yet visualized.
    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }

    #endregion

    //not a lifecycle callback - this is really only useful for when you need to specifically react to the DataContext changing (it also might be the same context, just reassigned; it's essentially bound to a property setter.
    /// <summary>
    /// Called when the DataContext is updated. We use this to connect our
    /// UI-specific interactions to the ViewModel.
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    }

    #region Overrides of Control

    
    //onenabled - called whenever the control is "visually added" (shown, made active, when its window is opened, etc)
    //ideal place to subscribe to events, and set up logic that needs to run every time the control is displayed.
    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        //together with the call in OnUnloaded, this enables the entire window to use a folder picker, as it requires access to a visual element, which the VM cannot directly touch.
        
        MainViewModel?.OpenFolderPicker.RegisterHandler(
            pickerTitle => PlumbobFileSystem.PickFolder(this, pickerTitle));
        _loadedTrove.AddCleanup("folder-picker-sub", 
            () => MainViewModel?.OpenFolderPicker.UnregisterHandler());
    }

    //ondisabled - called whenever the control is "visually removed" (hidden, made inactive, when its window is closed, etc)    
    //ideal place to unsubscribe from events, and set up logic that needs to run every time the control is hidden.
    /// <summary>
    /// Cleanup logic when the Window is removed from the visual tree.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _loadedTrove.Dispose();
    }
    
    #endregion
    

}