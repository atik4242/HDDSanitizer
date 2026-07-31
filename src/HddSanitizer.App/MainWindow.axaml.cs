using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HddSanitizer.Core;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;

namespace HddSanitizer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly CertificateWriter _certWriter;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        _certWriter = new CertificateWriter();
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDrivesAsync();
        BtnErase.IsEnabled = false;
    }

    private void OnDriveSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            BtnErase.IsEnabled = SanitizerSafetyGuard.CanErase(selectedDrive);
        }
        else
        {
            BtnErase.IsEnabled = false;
        }
    }

    private async void OnEraseClick(object? sender, RoutedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            var dialog = new ConfirmEraseWindow(selectedDrive);
            await dialog.ShowDialog(this);

            if (dialog.IsConfirmed)
            {
                string certPath = await _certWriter.GenerateCertificateAsync(selectedDrive);

                var msg = new Window
                {
                    Width = 480, Height = 180,
                    Title = "Löschzertifikat Erstellt",
                    Content = new TextBlock 
                    { 
                        Text = $"Löschvorgang für {selectedDrive.SerialNumber} erfolgreich abgeschlossen!\n\nAudit-Zertifikat wurde gespeichert unter:\n{Path.GetFullPath(certPath)}",
                        Margin = new Avalonia.Thickness(20),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                await msg.ShowDialog(this);
            }
        }
    }
}
