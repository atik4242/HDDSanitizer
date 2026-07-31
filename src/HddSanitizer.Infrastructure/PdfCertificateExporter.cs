using System;
using System.IO;
using HddSanitizer.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HddSanitizer.Infrastructure;

public class PdfCertificateExporter
{
    public static string ExportToPdf(ErasureCertificate cert, string outputDirectory = "logs")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        string fileName = $"Zertifikat_{cert.SerialNumber}_{cert.TimestampUtc:yyyyMMdd}.pdf";
        string filePath = Path.Combine(outputDirectory, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("HDD SANITIZER ENTERPRISE").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text("OFFIZIELLES DATENLÖSCH-ZERTIFIKAT").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken1);
                        });
                    });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Text($"Zertifikat-ID: {cert.CertificateId}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Ausstellungsdatum: {cert.TimestampUtc:dd.MM.yyyy HH:mm:ss} UTC").FontSize(10).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(15).Text("Laufwerks-Details").FontSize(14).Bold();

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(140);
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Modell:").Bold();
                        table.Cell().Text(cert.ModelName);

                        table.Cell().Text("Seriennummer:").Bold();
                        table.Cell().Text(cert.SerialNumber);

                        table.Cell().Text("Kapazität:").Bold();
                        table.Cell().Text($"{Math.Round(cert.CapacityBytes / (1024.0 * 1024.0 * 1024.0 * 1024.0), 2)} TB ({cert.CapacityBytes:N0} Bytes)");

                        table.Cell().Text("Löschmethode:").Bold();
                        table.Cell().Text(cert.ErasureMethod);

                        table.Cell().Text("Durchgeführt von:").Bold();
                        table.Cell().Text(cert.PerformedBy);
                    });

                    col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Green.Medium).Background(Colors.Green.Lighten5).Padding(10).Column(c =>
                    {
                        c.Item().Text("STATUS: ERFOLGREICH / SANITIZED").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                        c.Item().Text("Sämtliche Datenblöcke auf dem angegebenen Datenträger wurden unwiederbringlich überschrieben bzw. sicher gelöscht.").FontSize(9).FontColor(Colors.Grey.Darken3);
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text("HDD Sanitizer Suite • Audit-Trail verifiziert")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf(filePath);

        return filePath;
    }
}
