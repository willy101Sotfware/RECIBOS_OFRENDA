using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;

namespace RECIBOS_OFRENDA
{
    internal static class FacturaRenderer
    {
        public static string RenderFactura(FacturaData ts, string outputPath, int width = 0)
        {
            var culture = new CultureInfo("es-CO");
            // Parámetros desde configuración (con defaults adecuados a W80)
            int paperWidth = width > 0 ? width : AppConfig.PaperWidthPx; // 576 para 80mm, 384 para 58mm
            int leftMargin = AppConfig.MarginPx;
            int rightMargin = AppConfig.MarginPx;
            int yPos = 16;
            using var bmp = new Bitmap(paperWidth, 4000);
            bmp.SetResolution(203, 203);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using var blackBrush = new SolidBrush(Color.Black);
            using var pen = new Pen(Color.Black, 1);
            // Preferir tamaños en píxeles si están definidos (>0), de lo contrario usar puntos
            using var headerFont = AppConfig.HeaderFontPx > 0
                ? new Font("Arial", AppConfig.HeaderFontPx, FontStyle.Bold, GraphicsUnit.Pixel)
                : new Font("Arial", AppConfig.HeaderFontSize, FontStyle.Bold, GraphicsUnit.Point);
            using var normalFont = AppConfig.NormalFontPx > 0
                ? new Font("Arial", AppConfig.NormalFontPx, FontStyle.Regular, GraphicsUnit.Pixel)
                : new Font("Arial", AppConfig.NormalFontSize, FontStyle.Regular, GraphicsUnit.Point);
            using var smallFont = AppConfig.SmallFontPx > 0
                ? new Font("Arial", AppConfig.SmallFontPx, FontStyle.Regular, GraphicsUnit.Pixel)
                : new Font("Arial", AppConfig.SmallFontSize, FontStyle.Regular, GraphicsUnit.Point);

            float lineHeight = normalFont.GetHeight(g) + 3; // más espacio para mejorar legibilidad en térmica

            // Logo (opcional)
            if (File.Exists(AppConfig.ThermalLogoFullPath))
            {
                using var logo = Image.FromFile(AppConfig.ThermalLogoFullPath);
                int usableWidth = paperWidth - leftMargin - rightMargin;
                var maxPercentW = (int)Math.Floor(usableWidth * AppConfig.LogoMaxPercent);

                // convertir límites físicos en mm a píxeles a 203 dpi
                const double dpi = 203.0;
                int maxMmWpx = (int)Math.Round(AppConfig.LogoMaxWidthMm / 25.4 * dpi);
                int maxMmHpx = (int)Math.Round(AppConfig.LogoMaxHeightMm / 25.4 * dpi);

                int maxW = Math.Min(maxPercentW, maxMmWpx);
                if (maxW <= 0) maxW = maxPercentW;

                var ratio = (double)logo.Height / logo.Width;
                int lw = Math.Min(maxW, logo.Width);
                int lh = (int)Math.Round(lw * ratio);
                if (maxMmHpx > 0 && lh > maxMmHpx)
                {
                    lh = maxMmHpx;
                    lw = (int)Math.Round(lh / ratio);
                }

                // Asegurar que no se salga del área útil
                if (lw > usableWidth) lw = usableWidth;
                if (lh <= 0) lh = (int)Math.Max(1, usableWidth * ratio);

                var xLogo = leftMargin + (usableWidth - lw) / 2 + AppConfig.LogoOffsetXPx;
                g.DrawImage(logo, new Rectangle(xLogo, yPos, lw, lh));
                yPos += lh + 6;
            }

            // Encabezado centrado: "LA OFRENDA - FACTURA"
            string header = "LA OFRENDA - FACTURA";
            var headerSize = g.MeasureString(header, headerFont);
            float headerX = (paperWidth - headerSize.Width) / 2f;
            g.DrawString(header, headerFont, blackBrush, headerX, yPos);
            yPos += (int)(lineHeight * 1.5f);

            // Línea separadora
            g.DrawLine(pen, leftMargin, yPos + 7, paperWidth - rightMargin, yPos + 7);
            yPos += (int)lineHeight;

            // Campos exactos
            DrawLine(g, $"ID Trans: {ts.IdTransaccionApi}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);
            DrawLine(g, $"Documento : {Safe(ts.Documento, "N/A")}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);
            DrawLine(g, $"Referencia: {Safe(ts.Referencia, "N/A")}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);

            var cliente = Truncate(ts.Cliente, 30);
            DrawLine(g, $"Cliente : {cliente}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);

            var producto = Truncate(Safe(ts.TipoRecaudo, "N/A"), 30);
            DrawLine(g, $"Producto: {producto}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);

            DrawLine(g, $"Pago    : {GetTipoPagoDescripcion(ts.TipoPago)}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);

            // Separador
            g.DrawLine(pen, leftMargin, yPos + 7, paperWidth - rightMargin, yPos + 7);
            yPos += (int)lineHeight;

            DrawLine(g, $"Valor Total    : ${ts.Total.ToString("N0", culture)}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);
            DrawLine(g, $"Valor Ingresado  : ${ts.TotalIngresado.ToString("N0", culture)}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);
            DrawLine(g, $"Valor Devuelto: ${ts.TotalDevuelta.ToString("N0", culture)}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);

            // Separador
            g.DrawLine(pen, leftMargin, yPos + 7, paperWidth - rightMargin, yPos + 7);
            yPos += (int)lineHeight;

            DrawLine(g, $"Estado: {Safe(ts.EstadoTransaccionVerb, "")}", normalFont, blackBrush, leftMargin, ref yPos, lineHeight);
            var fechaText = string.IsNullOrWhiteSpace(ts.FechaTexto) ? DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") : ts.FechaTexto;
            DrawLine(g, $"Fecha : {fechaText}", smallFont, blackBrush, leftMargin, ref yPos, lineHeight * 1.6f);

            // Gracias centrado
            string thanks = "Gracias por su pago!";
            var thanksSize = g.MeasureString(thanks, normalFont);
            float thanksX = (paperWidth - thanksSize.Width) / 2f;
            g.DrawString(thanks, normalFont, blackBrush, thanksX, yPos);
            yPos += (int)(lineHeight * 2);

            // Padding final
            DrawLine(g, " ", smallFont, blackBrush, leftMargin, ref yPos, lineHeight);

            // Recortar y guardar
            int finalHeight = yPos + 16;
            using var finalBmp = new Bitmap(paperWidth, finalHeight);
            finalBmp.SetResolution(203, 203);
            using (var g2 = Graphics.FromImage(finalBmp))
            {
                g2.Clear(Color.White);
                g2.DrawImage(bmp, new Rectangle(0, 0, paperWidth, finalHeight), new Rectangle(0, 0, paperWidth, finalHeight), GraphicsUnit.Pixel);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            finalBmp.Save(outputPath, System.Drawing.Imaging.ImageFormat.Bmp);
            return outputPath;
        }

        private static void DrawLine(Graphics g, string text, Font font, Brush brush, int x, ref int y, float lineHeight)
        {
            g.DrawString(text, font, brush, x, y);
            y += (int)lineHeight;
        }

        private static string Safe(string? v, string fallback) => string.IsNullOrWhiteSpace(v) ? fallback : v!;

        private static string Truncate(string? v, int max)
        {
            var s = v ?? string.Empty;
            if (s.Length <= max) return s;
            if (max <= 3) return s.Substring(0, max);
            return s.Substring(0, max - 3) + "...";
        }

        private static string GetTipoPagoDescripcion(string? tipo)
        {
            var t = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return t switch
            {
                "EFECTIVO" => "Efectivo",
                "TARJETA" => "Tarjeta",
                "TRANSFERENCIA" => "Transferencia",
                _ => string.IsNullOrWhiteSpace(tipo) ? "N/A" : tipo
            };
        }
    }
}
