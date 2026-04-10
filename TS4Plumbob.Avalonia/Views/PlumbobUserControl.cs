using Avalonia.Controls;
using TS4Plumbob.Avalonia.ViewModels;

namespace TS4Plumbob.Avalonia.Views;

public class PlumbobUserControl : UserControl
{
    public RootViewModel? MainViewModel => DataContext as RootViewModel;
}