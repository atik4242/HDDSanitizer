using System.IO;
using System.Threading.Tasks;
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

    private async void OnShowCertificatesClick(object? sender, RoutedEventArgs e)
    {
        var viewer = new CertificateViewerWindow();
        await viewer.ShowDialog(this);
    }

    private void OnDriveSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            BtnErase.IsEnabled = SanitizerSafetyGuard.CanErase(selectedDrive);
            
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
            var confirmDialog = new ConfirmEraseWindow(selectedDrive);
            await confirmDialog.ShowDialog(this);

            if (confirmDialog.IsConfirmed)
            {
                var progressWindow = new EraseProgressWindow($"{selectedDrive.ModelName} ({selectedDrive.DevicePath})", confirmDialog.SelectedMethodName);
                progressWindow.Show(this);

                bool success = await Task.Run(async () => await _eraser.ExecuteErasureAsync(selectedDrive, confirmDialog.SelectedMethodName));

                progressWindow.Close();

                if (success)
                {
                    string certPath = await _certWriter.GenerateCertificateAsync(selectedDrive, confirmDialog.SelectedMethodName);

                    var msg = new Window
                    {
                        Width = 500, Height = 200,
                        Title = "Löschvorgang Erfolgreich",
                        Content = new TextBlock 
                        { 
                            Text = $"Löschvorgang ({confirmDialog.SelectedMethodName}) für {selectedDrive.SerialNumber} erfolgreich abgeschlossen!\n\nAudit-Zertifikat wurde gespeichert unter:\n{Path.GetFullPath(certPath)}",
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
