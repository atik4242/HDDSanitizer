using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;

namespace HddSanitizer.App;

public partial class CertificateViewerWindow : Window
{
    private readonly CertificateWriter _certWriter;

    public CertificateViewerWindow()
    {
        InitializeComponent();
        _certWriter = new CertificateWriter();
        GridCerts.DoubleTapped += OnGridDoubleTapped;
        _ = LoadCertificatesAsync();
    }

    private async System.Threading.Tasks.Task LoadCertificatesAsync()
    {
        var certs = await _certWriter.GetAllCertificatesAsync();
        GridCerts.ItemsSource = certs;
    }

    private async void OnGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (GridCerts.SelectedItem is ErasureCertificate cert)
        {
            var detailWindow = new CertificateDetailWindow(cert);
            await detailWindow.ShowDialog(this);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
