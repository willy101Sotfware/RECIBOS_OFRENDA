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

            // Forzar ancho de papel aproximado al del BMP (58mm=384px o 80mm=576px) convirtiendo a centésimas de pulgada @203 dpi
            try
            {
                const double dpi = 203.0;
                int widthHundredths = (int)Math.Round(AppConfig.PaperWidthPx / dpi * 100.0);
                // Altura grande para recibo (por ejemplo 200 mm)
                int heightHundredths = 20000; // 200 mm ≈ 7.87 in ≈ 787 hundredths; usamos más para evitar corte
                doc.DefaultPageSettings.PaperSize = new PaperSize("CustomThermal", widthHundredths, heightHundredths);
            }
            catch { /* si el driver no acepta custom size, seguimos con el default */ }

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

                    // Escalar SOLO al ancho imprimible manteniendo aspecto
                    float scale = Math.Min((float)printableWidthPx / bmp.Width, (float)printableHeightPx / bmp.Height);
                    if (scale > 1f) scale = 1f; // no ampliar
                    if (scale <= 0) scale = 1f;
                    int drawWidthPx = (int)Math.Floor(bmp.Width * scale);
                    int drawHeightPx = (int)Math.Floor(bmp.Height * scale);

                    // Alinear A LA IZQUIERDA del área imprimible y compensar el offset del driver
                    int x = -printableLeftPx + AppConfig.PrintOffsetXPx; // mover a la izquierda del papel
                    int y = -printableTopPx; // alinear arriba

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
