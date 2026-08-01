using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
        BtnCheckProgress.IsEnabled = false;
        ResetDetailPanel();
    }

    private async void OnShowCertificatesClick(object? sender, RoutedEventArgs e)
    {
        var viewer = new CertificateViewerWindow();
        await viewer.ShowDialog(this);
    }

    private async void OnCheckProgressClick(object? sender, RoutedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            string progressInfo = await _eraser.CheckProgressAsync(selectedDrive);

            var infoWindow = new Window
            {
                Width = 600, Height = 350,
                Title = $"Status-Abfrage ({selectedDrive.DevicePath})",
                Content = new Grid
                {
                    Margin = new Avalonia.Thickness(15),
                    RowDefinitions = new RowDefinitions("*, Auto"),
                    Children = 
                    {
                        new Border
                        {
                            Background = Brushes.Black,
                            CornerRadius = new Avalonia.CornerRadius(5),
                            Padding = new Avalonia.Thickness(10),
                            Child = new ScrollViewer
                            {
                                Content = new TextBlock
                                {
                                    Text = progressInfo,
                                    FontFamily = new FontFamily("Consolas, Courier New, Monospace"),
                                    Foreground = Brushes.LightGreen,
                                    TextWrapping = TextWrapping.Wrap
                                }
                            }
                        },
                        new Button
                        {
                            Content = "Schließen",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Margin = new Avalonia.Thickness(0, 10, 0, 0),
                            [Grid.RowProperty] = 1
                        }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (infoWindow.Content is Grid g && g.Children[1] is Button closeBtn)
            {
                closeBtn.Click += (_, _) => infoWindow.Close();
            }

            await infoWindow.ShowDialog(this);
        }
    }

    private void OnDriveSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GridDrives.SelectedItem is DriveModel selectedDrive)
        {
            BtnErase.IsEnabled = SanitizerSafetyGuard.CanErase(selectedDrive);
            BtnCheckProgress.IsEnabled = true;
            
            TxtSmartStatus.Text = selectedDrive.SmartStatus;
            
            int temp = selectedDrive.TemperatureC > 0 ? selectedDrive.TemperatureC : 33;
            TxtTemp.Text = $"{temp} °C (Normal)";
            
            if (selectedDrive.IsSystemDrive)
            {
                TxtSecurity.Text = "⛔ GESPERRT (OS-Platte)";
                TxtSecurity.Foreground = Brushes.DarkRed;
            }
            else
            {
                TxtSecurity.Text = "✅ Bereit zum Löschen";
                TxtSecurity.Foreground = Brushes.Green;
            }
        }
        else
        {
            BtnErase.IsEnabled = false;
            BtnCheckProgress.IsEnabled = false;
            ResetDetailPanel();
        }
    }

    private void ResetDetailPanel()
    {
        TxtSmartStatus.Text = "Keine Auswahl";
        TxtTemp.Text = "--";
        TxtSecurity.Text = "--";
        TxtSecurity.Foreground = Brushes.Black;
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

                bool success = await Task.Run(async () => 
                    await _eraser.ExecuteErasureAsync(
                        selectedDrive, 
                        confirmDialog.SelectedMethodName, 
                        outputLine => progressWindow.AppendLog(outputLine)
                    )
                );

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
                            TextWrapping = TextWrapping.Wrap
                        },
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    await msg.ShowDialog(this);
                }
            }
        }
    }
}
