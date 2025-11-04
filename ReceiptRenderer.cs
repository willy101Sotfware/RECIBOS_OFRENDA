using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace RECIBOS_OFRENDA
{
    internal static class ReceiptRenderer
    {
        public static string RenderToBmp(ReceiptData data, string outputPath, int width = 576)
        {
            var padding = 16;
            var y = padding;
            using var bmp = new Bitmap(width, 4000);
            bmp.SetResolution(203, 203);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Logo
            if (File.Exists(AppConfig.ThermalLogoFullPath))
            {
                using var logo = Image.FromFile(AppConfig.ThermalLogoFullPath);
                var maxLogoWidth = width - padding * 2;
                var ratio = (double)logo.Height / logo.Width;
                var logoW = Math.Min(maxLogoWidth, logo.Width);
                var logoH = (int)(logoW * ratio);
                var xLogo = (width - logoW) / 2;
                g.DrawImage(logo, new Rectangle(xLogo, y, logoW, logoH));
                y += logoH + 10;
            }

            using var fontTitle = new Font("Arial", 16, FontStyle.Bold);
            using var font = new Font("Arial", 10, FontStyle.Regular);
            using var fontBold = new Font("Arial", 10, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Black);

            var centerFormat = new StringFormat { Alignment = StringAlignment.Center };
            var leftFormat = new StringFormat { Alignment = StringAlignment.Near };

            // Título
            g.DrawString(data.Titulo, fontTitle, brush, new RectangleF(0, y, width, fontTitle.Height), centerFormat);
            y += fontTitle.Height + 6;

            // Fecha y No. Recibo
            var headerLine = $"Fecha: {data.Fecha}    No: {data.NumeroRecibo}";
            g.DrawString(headerLine, font, brush, new RectangleF(padding, y, width - padding * 2, font.Height), leftFormat);
            y += font.Height + 6;

            // Separador
            DrawSeparator(g, ref y, width, padding);

            // Datos
            y = DrawPair(g, "Cliente :", data.Nombre, fontBold, font, brush, width, padding, y);
            y = DrawPair(g, "Documento:", data.Documento, fontBold, font, brush, width, padding, y);
            y = DrawPair(g, "Concepto:", data.Concepto, fontBold, font, brush, width, padding, y);
            y = DrawPair(g, "Valor:", data.Valor, fontBold, font, brush, width, padding, y);
            y = DrawPair(g, "Referencia:", data.Referencia, fontBold, font, brush, width, padding, y);

            // Separador
            DrawSeparator(g, ref y, width, padding);

            // Pie
            g.DrawString(data.Pie, font, brush, new RectangleF(padding, y, width - padding * 2, font.Height * 2), centerFormat);
            y += font.Height + 10;

            // recortar imagen a la altura usada
            var finalHeight = y + padding;
            using var finalBmp = new Bitmap(width, finalHeight);
            finalBmp.SetResolution(203, 203);
            using (var g2 = Graphics.FromImage(finalBmp))
            {
                g2.Clear(Color.White);
                g2.DrawImage(bmp, new Rectangle(0, 0, width, finalHeight), new Rectangle(0, 0, width, finalHeight), GraphicsUnit.Pixel);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            finalBmp.Save(outputPath, ImageFormat.Bmp);
            return outputPath;
        }

        private static int DrawPair(Graphics g, string label, string value, Font fontLabel, Font fontValue, Brush brush, int width, int padding, int y)
        {
            var leftFormat = new StringFormat { Alignment = StringAlignment.Near };
            var labelWidth = (int)(width * 0.32);
            g.DrawString(label, fontLabel, brush, new RectangleF(padding, y, labelWidth, fontLabel.Height), leftFormat);
            g.DrawString(value ?? string.Empty, fontValue, brush, new RectangleF(padding + labelWidth + 6, y, width - padding * 2 - labelWidth - 6, fontValue.Height), leftFormat);
            return y + Math.Max(fontLabel.Height, fontValue.Height) + 4;
        }

        private static void DrawSeparator(Graphics g, ref int y, int width, int padding)
        {
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, padding, y, width - padding, y);
            y += 6;
        }
    }
}
