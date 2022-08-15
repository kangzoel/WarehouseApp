using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using WarehouseApp.Models;

namespace WarehouseApp.Utilities.Pdf;

internal class StockOutputLogExporter : PdfExporter
{
    protected override string[] HeaderLines { get; } = new[] { "Riwayat Barang Keluar" };
    protected override string[] TableColumns { get; } =
        new[] { "No.", "Tanggal", "Barcode", "Nama", "Jumlah", "Satuan", "Kedaluwarsa" };
    protected override float[] TableColumnWidths { get; } =
        new float[] { 0, .175f, 0, 1, 0, 0, .175f };

    private readonly List<StockOutputLog> _data;

    public StockOutputLogExporter(List<StockOutputLog> data)
    {
        _data = data;
    }

    protected override void GenerateTableContent(PdfDocument pdf, Table table)
    {
        foreach (
            var (stockInputLog, index) in _data.Select(
                (stockInputLog, index) => (stockInputLog, index)
            )
        )
        {
            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(( index + 1 ).ToString()).SetTextAlignment(TextAlignment.RIGHT)
                    )
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            var pCreatedAt = new Paragraph(stockInputLog.CreatedAt.ToString("dd-MM-yyyy"));
            pCreatedAt.SetProperty(Property.OVERFLOW_X, OverflowPropertyValue.VISIBLE);

            table.AddCell(
                new Cell()
                    .Add(pCreatedAt.SetTextAlignment(TextAlignment.CENTER))
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(stockInputLog.Stock!.Item!.Barcode).SetTextAlignment(
                            TextAlignment.CENTER
                        )
                    )
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            table.AddCell(
                new Cell()
                    .Add(new Paragraph(stockInputLog.Stock!.Item!.Name))
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(stockInputLog.Amount.ToString()).SetTextAlignment(
                            TextAlignment.RIGHT
                        )
                    )
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            table.AddCell(
                new Cell()
                    .Add(new Paragraph(stockInputLog.Stock!.Item!.Quantifier))
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(
                            stockInputLog.Stock!.ExpirationDate.ToString("dd-MM-yyyy")
                        ).SetTextAlignment(TextAlignment.CENTER)
                    )
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );
        }
    }
}
