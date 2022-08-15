using System.Collections.Generic;
using System.Linq;
using iText.Barcodes;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using WarehouseApp.Models;

namespace WarehouseApp.Utilities.Pdf;

internal class ItemsExporter : PdfExporter
{
    protected override string[] HeaderLines { get; } = new[] { "DAFTAR BARANG" };
    protected override string[] TableColumns { get; } =
        new[] { "No.", "Barcode", "Nama", "Jml. Stok", "Satuan" };
    protected override float[] TableColumnWidths { get; } = new[] { 0, 0, 1f, 0, 0 };

    private readonly List<Item> _data;

    public ItemsExporter(List<Item> data)
    {
        _data = data;
    }

    protected override void GenerateTableContent(PdfDocument pdf, Table table)
    {
        foreach (var (item, index) in _data.Select((data, index) => (data, index)))
        {
            var barcode = new BarcodeEAN(pdf);

            barcode.SetCodeType(BarcodeEAN.EAN13);
            barcode.SetCode(item.Barcode);

            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(( index + 1 ).ToString()).SetHorizontalAlignment(
                            HorizontalAlignment.RIGHT
                        )
                    )
                    .SetPaddings(2, 6, 2, 6)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
            );

            table.AddCell(
                new Cell()
                    .Add(new Image(barcode.CreateFormXObject(null, null, pdf)))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPaddings(6, 6, 4, 6)
            );

            table.AddCell(
                new Cell()
                    .Add(new Paragraph(item.Name))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPaddings(2, 6, 2, 6)
            );

            table.AddCell(
                new Cell()
                    .Add(
                        new Paragraph(item.TotalAmount.ToString()).SetHorizontalAlignment(
                            HorizontalAlignment.RIGHT
                        )
                    )
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPaddings(2, 6, 2, 6)
            );

            table.AddCell(
                new Cell()
                    .Add(new Paragraph(item.Quantifier))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPaddings(2, 6, 2, 6)
            );
        }
    }
}
