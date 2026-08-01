using Avalonia.Controls;
using Avalonia.Threading;

namespace HddSanitizer.App;

public partial class EraseProgressWindow : Window
{
    public EraseProgressWindow()
    {
        InitializeComponent();
    }

    public EraseProgressWindow(string driveInfo, string method) : this()
    {
        TxtStatus.Text = $"Lösche {driveInfo}...";
        TxtDetail.Text = $"Methode: {method}";
    }

    public void AppendLog(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TxtLiveLog.Text += $"\n{text}";
            LogScrollViewer.ScrollToEnd();
        });
    }
}
