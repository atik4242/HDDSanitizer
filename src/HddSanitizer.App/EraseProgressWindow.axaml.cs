using System;
using Avalonia.Controls;
using Avalonia.Threading;
using HddSanitizer.Domain;

namespace HddSanitizer.App;

public partial class EraseProgressWindow : Window
{
    public EraseProgressWindow()
    {
        InitializeComponent();
    }

    public EraseProgressWindow(string driveInfo, string methodName) : this()
    {
        // Falls TextBlocks/Controls im XAML existieren, können diese hier initialisiert werden
        Title = $"Löschvorgang läuft: {driveInfo}";
    }

    public void UpdateProgress(ErasureProgress progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Aktualisiert UI-Elemente wie ProgressBar oder Labels, sofern im XAML vorhanden
            // Beispiel:
            // ProgressBarErase.Value = progress.Percentage;
            // TxtStatus.Text = $"{progress.Percentage:F1}% | {progress.SpeedMBs:F1} MB/s | Restzeit: {progress.RemainingTime:hh\\:mm\\:ss}";
        }, DispatcherPriority.Background);
    }

    public void AppendLog(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Falls ein Terminal/TextBlock für Logs existiert
            // TxtLog.Text += line + "\n";
        }, DispatcherPriority.Background);
    }
}