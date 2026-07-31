using Avalonia.Controls;
using Avalonia.Interactivity;
using HddSanitizer.Core;
using HddSanitizer.Domain;

namespace HddSanitizer.App;

public partial class ConfirmEraseWindow : Window
{
    private readonly DriveModel? _targetDrive;

    public bool IsConfirmed { get; private set; }

    public ConfirmEraseWindow()
    {
        InitializeComponent();
    }

    public ConfirmEraseWindow(DriveModel drive) : this()
    {
        _targetDrive = drive;
        TxtModel.Text = $"Modell: {drive.ModelName}";
        TxtPath.Text = $"Pfad: {drive.DevicePath} ({drive.CapacityTB} TB)";
        TxtSerial.Text = $"Erforderliche Seriennummer: {drive.SerialNumber}";
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (_targetDrive != null && SanitizerSafetyGuard.VerifySerialNumber(_targetDrive, InputSerial.Text ?? ""))
        {
            IsConfirmed = true;
            Close();
        }
        else
        {
            TxtError.Text = "Falsche Seriennummer! Vorgang abgebrochen.";
            TxtError.IsVisible = true;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }
}
