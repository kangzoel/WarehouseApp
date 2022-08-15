using System;
using System.Globalization;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Path = System.IO.Path;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = iText.Layout.Properties.VerticalAlignment;

namespace WarehouseApp.Utilities.Pdf;

internal abstract class PdfExporter
{
    protected readonly PdfFont ROMAN = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
    protected readonly PdfFont ROMAN_BOLD = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

    protected abstract string[] HeaderLines { get; }
    protected abstract string[] TableColumns { get; }
    protected abstract float[] TableColumnWidths { get; }
    protected static string DateString
    {
        get
        {
            var idCi = new CultureInfo("id-ID");
            return DateTime.Now.ToString("MMMM yyyy", idCi);
        }
    }

    public void Save(string path)
    {
        EnsureDirectoryExists(path);

        // instantiate pdf doc
        var pdf = new PdfDocument(new PdfWriter(path));
        var doc = new Document(pdf).SetFont(ROMAN).SetFontSize(12);

        pdf.SetDefaultPageSize(PageSize.A4.Rotate());
        GenerateHeaderLines(doc);
        GenerateHeaderInfo(doc);
        GenerateTable(pdf, doc);

        doc.Close();
    }

    private static void GenerateHeaderInfo(Document doc)
    {
        doc.Add(new Paragraph($"Tanggal\t: {DateTime.Now:dd-MM-yyyy}"));
    }

    protected abstract void GenerateTableContent(PdfDocument pdf, Table table);

    private void GenerateHeaderLines(Document doc)
    {
        foreach (var line in HeaderLines)
        {
            doc.Add(
                new Paragraph(line)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(ROMAN_BOLD)
                    .SetFontSize(14)
                    .SetMultipliedLeading(1)
            );
        }

        doc.Add(
            new Paragraph(DateString)
                .SetTextAlignment(TextAlignment.CENTER)
                 .SetFont(ROMAN_BOLD)
                .SetFontSize(14)
                .SetMultipliedLeading(1)
        );
    }

    private void GenerateTable(PdfDocument pdf, Document doc)
    {
        var table = new Table(UnitValue.CreatePercentArray(TableColumnWidths))
            .UseAllAvailableWidth();

        GenerateTableHeader(table);
        GenerateTableContent(pdf, table);

        doc.Add(table);
    }

    private void GenerateTableHeader(Table table)
    {
        foreach (var text in TableColumns)
        {
            table.AddCell(
                new Cell()
                    .Add(new Paragraph(text).SetFont(ROMAN_BOLD))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBackgroundColor(new DeviceGray(.9f))
                    .SetPaddings(2, 6, 2, 6)
            );
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        var outputDir = Path.GetDirectoryName(path);
        Directory.CreateDirectory(outputDir!);
    }
}
