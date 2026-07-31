using Avalonia.Controls;
using Avalonia.Interactivity;
using HddSanitizer.Infrastructure;

namespace HddSanitizer.App;

public partial class CertificateViewerWindow : Window
{
    private readonly CertificateWriter _certWriter;

    public CertificateViewerWindow()
    {
        InitializeComponent();
        _certWriter = new CertificateWriter();
        _ = LoadCertificatesAsync();
    }

    private async System.Threading.Tasks.Task LoadCertificatesAsync()
    {
        var certs = await _certWriter.GetAllCertificatesAsync();
        GridCerts.ItemsSource = certs;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
