using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HddSanitizer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDrivesAsync();
    }
}
