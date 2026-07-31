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
        ResetDetailPanel();
    }

    private void OnDriveSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            BtnErase.IsEnabled = SanitizerSafetyGuard.CanErase(selectedDrive);
            
            // SMART & Details aktualisieren
            TxtSmartStatus.Text = selectedDrive.SmartStatus;
            TxtTemp.Text = selectedDrive.TemperatureC > 0 ? $"{selectedDrive.TemperatureC} °C" : "N/A";
            
            if (selectedDrive.IsSystemDrive)
            {
                TxtSecurity.Text = "⛔ GESPERRT (OS-Platte)";
                TxtSecurity.Foreground = Avalonia.Media.Brushes.Red;
            }
            else
            {
                TxtSecurity.Text = "✅ Bereit zum Löschen";
                TxtSecurity.Foreground = Avalonia.Media.Brushes.LightGreen;
            }
        }
        else
        {
            BtnErase.IsEnabled = false;
            ResetDetailPanel();
        }
    }

    private void ResetDetailPanel()
    {
        TxtSmartStatus.Text = "Keine Auswahl";
        TxtTemp.Text = "--";
        TxtSecurity.Text = "--";
        TxtSecurity.Foreground = Avalonia.Media.Brushes.White;
    }

    private async void OnEraseClick(object? sender, RoutedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            var dialog = new ConfirmEraseWindow(selectedDrive);
            await dialog.ShowDialog(this);

            if (dialog.IsConfirmed)
            {
                bool success = await _eraser.ExecuteErasureAsync(selectedDrive, dialog.SelectedMethodName);

                if (success)
                {
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
