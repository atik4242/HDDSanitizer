using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HddSanitizer.Core;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;
using HddSanitizer.SeaChest;

namespace HddSanitizer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly CertificateWriter _certWriter;
    private readonly SeaChestEraser _eraser;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        _certWriter = new CertificateWriter();
        _eraser = new SeaChestEraser();
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
                // 1. Führe den Löschbefehl über openSeaChest CLI aus
                bool success = await _eraser.ExecuteErasureAsync(selectedDrive, dialog.SelectedMethodName);

                if (success)
                {
                    // 2. Erzeuge das Audit-Zertifikat
                    string certPath = await _certWriter.GenerateCertificateAsync(selectedDrive, dialog.SelectedMethodName);

                    var msg = new Window
                    {
                        Width = 500, Height = 200,
                        Title = "Löschvorgang Erfolgreich",
                        Content = new TextBlock 
                        { 
                            Text = $"Löschvorgang ({dialog.SelectedMethodName}) für {selectedDrive.SerialNumber} erfolgreich ausgeführt!\n\nAudit-Zertifikat wurde gespeichert unter:\n{Path.GetFullPath(certPath)}",
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
}
