using System;
using System.Drawing;
using System.Drawing.Printing;

namespace RECIBOS_OFRENDA
{
    internal static class WindowsBmpPrinter
    {
        public static void Print(string imagePath, string? printerName = null)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) throw new ArgumentException("imagePath vacío");
            if (!System.IO.File.Exists(imagePath)) throw new ArgumentException($"No existe el archivo: {imagePath}");

            using var bmp = Image.FromFile(imagePath);
            using var doc = new PrintDocument();
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                doc.PrinterSettings.PrinterName = printerName;
            }

            // Evitar diálogo
            doc.PrintController = new StandardPrintController();

            // Márgenes a cero para térmica
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            // Usar papel y resolución por defecto del driver. Escalar al área imprimible.

            doc.PrintPage += (s, e) =>
            {
                try
                {
                    var printable = e.PageSettings.PrintableArea; // hundredths of an inch
                    float targetDpiX = e.Graphics.DpiX;
                    float targetDpiY = e.Graphics.DpiY;

                    int printableWidthPx  = (int)Math.Round(printable.Width  / 100f * targetDpiX);
                    int printableHeightPx = (int)Math.Round(printable.Height / 100f * targetDpiY);
                    int printableLeftPx   = (int)Math.Round(printable.X      / 100f * targetDpiX);
                    int printableTopPx    = (int)Math.Round(printable.Y      / 100f * targetDpiY);

                    // Escalar para caber al ancho imprimible manteniendo aspecto
                    float scale = Math.Min((float)printableWidthPx / bmp.Width, (float)printableHeightPx / bmp.Height);
                    // Importante: no ampliar nunca, solo reducir si es necesario
                    if (scale > 1f) scale = 1f;
                    if (scale <= 0) scale = 1f;
                    int drawWidthPx = (int)Math.Floor(bmp.Width * scale);
                    int drawHeightPx = (int)Math.Floor(bmp.Height * scale);

                    // Centrar horizontalmente, alinear arriba
                    int x = printableLeftPx + Math.Max(0, (printableWidthPx - drawWidthPx) / 2);
                    int y = printableTopPx;

                    e.Graphics.DrawImage(bmp, new Rectangle(x, y, drawWidthPx, drawHeightPx));
                    e.HasMorePages = false;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Fallo al dibujar la imagen para impresión: {ex.Message}");
                }
            };

            doc.Print();
        }
    }
}
