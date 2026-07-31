using Avalonia.Controls;

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
        TxtDetail.Text = $"Methode: {method}\nBitte das Gerät nicht trennen.";
    }
}
