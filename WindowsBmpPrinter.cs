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

            // Mapear píxeles del BMP a pulgadas usando 203 dpi (térmica típica)
            const float printerDpi = 203f; // puntos por pulgada
            float widthInches = bmp.Width / printerDpi;
            float heightInches = bmp.Height / printerDpi;

            // Definir tamaño de papel según el tamaño real del BMP
            int widthHundredthsInch = (int)Math.Ceiling(widthInches * 100f);
            int heightHundredthsInch = (int)Math.Ceiling(heightInches * 100f) + 5; // pequeño margen extra

            // Algunas W80 aceptan ancho ~284 hundredths (≈72 mm imprimibles). Si el BMP es 576px, será ≈2.84"
            // Usar el ancho calculado; si el driver lo limita, seguirá usando el más cercano.
            var paper = new PaperSize("Receipt_Custom", widthHundredthsInch, heightHundredthsInch);
            doc.DefaultPageSettings.PaperSize = paper;

            // También intentar ajustar la resolución si el driver lo soporta
            try
            {
                foreach (PrinterResolution res in doc.PrinterSettings.PrinterResolutions)
                {
                    if (res.X == 203 && res.Y == 203)
                    {
                        doc.DefaultPageSettings.PrinterResolution = res;
                        break;
                    }
                }
            }
            catch { /* algunos drivers no exponen resoluciones */ }

            doc.PrintPage += (s, e) =>
            {
                try
                {
                    // Calcular tamaño de dibujo en píxeles destino según DPI real del dispositivo
                    float targetDpiX = e.Graphics.DpiX;
                    float targetDpiY = e.Graphics.DpiY;

                    // Mantener tamaño físico del BMP: px->pulgadas a 203 dpi, luego a px del dispositivo
                    int drawWidthPx = (int)Math.Round(widthInches * targetDpiX);
                    int drawHeightPx = (int)Math.Round(heightInches * targetDpiY);

                    // Usar área imprimible del driver para evitar recortes
                    var printable = e.PageSettings.PrintableArea; // hundredths of an inch
                    int printableWidthPx = (int)Math.Round(printable.Width / 100f * targetDpiX);
                    int printableLeftPx = (int)Math.Round(printable.X / 100f * targetDpiX);
                    int printableTopPx = (int)Math.Round(printable.Y / 100f * targetDpiY);

                    // Centrar horizontalmente
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
