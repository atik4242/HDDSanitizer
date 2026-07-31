using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;

namespace HddSanitizer.App;

public partial class CertificateDetailWindow : Window
{
    private readonly ErasureCertificate? _cert;

    public CertificateDetailWindow()
    {
        InitializeComponent();
    }

    public CertificateDetailWindow(ErasureCertificate cert) : this()
    {
        _cert = cert;
        TxtCertId.Text = $"ID: {cert.CertificateId}";
        TxtModel.Text = $"Modell: {cert.ModelName}";
        TxtSerial.Text = $"Seriennummer: {cert.SerialNumber}";
        TxtCapacity.Text = $"Kapazität: {System.Math.Round(cert.CapacityBytes / (1024.0 * 1024.0 * 1024.0 * 1024.0), 2)} TB";
        TxtMethod.Text = $"Methode: {cert.ErasureMethod}";
        TxtDate.Text = $"Zeitpunkt: {cert.TimestampUtc:dd.MM.yyyy HH:mm:ss} UTC";
    }

    private void OnExportPdfClick(object? sender, RoutedEventArgs e)
    {
        if (_cert != null)
        {
            string pdfPath = PdfCertificateExporter.ExportToPdf(_cert);
            TxtExportStatus.Text = $"✅ PDF gespeichert unter:\n{Path.GetFullPath(pdfPath)}";
            TxtExportStatus.IsVisible = true;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
