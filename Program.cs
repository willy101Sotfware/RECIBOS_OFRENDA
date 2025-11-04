using System;
using System.Globalization;
using System.IO;
using WPF_LA_OFRENDA_V1.Domain.Peripherals;

namespace RECIBOS_OFRENDA
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // Captura de datos EXACTOS para factura
            var ts = new FacturaData();
            Console.Write("ID Trans: ");
            ts.IdTransaccionApi = Console.ReadLine() ?? string.Empty;

            Console.Write("Documento: ");
            ts.Documento = Console.ReadLine() ?? string.Empty;

            Console.Write("Referencia: ");
            ts.Referencia = Console.ReadLine() ?? string.Empty;

            Console.Write("Cliente: ");
            ts.Cliente = Console.ReadLine() ?? string.Empty;

            Console.Write("Producto (Tipo de recaudo): ");
            ts.TipoRecaudo = Console.ReadLine() ?? string.Empty;

            Console.Write("Pago (EFECTIVO/TARJETA/TRANSFERENCIA): ");
            ts.TipoPago = Console.ReadLine() ?? string.Empty;

            var culture = new CultureInfo("es-CO");
            Console.Write("Valor Total: $");
            ts.Total = ParseDecimal(Console.ReadLine(), culture);

            Console.Write("Valor Ingresado: $");
            ts.TotalIngresado = ParseDecimal(Console.ReadLine(), culture);

            Console.Write("Valor Devuelto: $");
            ts.TotalDevuelta = ParseDecimal(Console.ReadLine(), culture);

            Console.Write("Estado: ");
            ts.EstadoTransaccionVerb = Console.ReadLine() ?? string.Empty;

            ts.FechaTexto = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", culture);

            Console.Write("Nombre de impresora Windows (vacío = predeterminada): ");
            var windowsPrinterName = Console.ReadLine();

            var receiptsDir = Path.Combine(AppContext.BaseDirectory, "Receipts");
            Directory.CreateDirectory(receiptsDir);
            var filePath = Path.Combine(receiptsDir, $"factura_{DateTime.Now:yyyyMMddHHmmss}.bmp");

            try
            {
                // Renderizar
                var saved = FacturaRenderer.RenderFactura(ts, filePath);
                Console.WriteLine($"Factura generada: {saved}");

                // Ruta de impresión según configuración
                if (AppConfig.UseWindowsPrinter)
                {
                    try
                    {
                        Console.WriteLine("Imprimiendo con impresora de Windows (centrado automático)...");
                        WindowsBmpPrinter.Print(saved, string.IsNullOrWhiteSpace(windowsPrinterName) ? null : windowsPrinterName);
                        Console.WriteLine("Enviado a la impresora de Windows.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fallo impresión Windows: {ex.Message}");
                    }
                }
                else
                {
                    // Intentar Msprintsdk primero
                    try
                    {
                        var status = PrintService.PrintBitmap(saved);
                        Console.WriteLine($"Intento de impresión: {status} - {PrintService.EvaluateStatus((int)status)}");

                        // Fallback: si Msprintsdk falla, usar PrintDocument de Windows
                        if (status != DefaultPrinterStatus.PrinterIsOk)
                        {
                            Console.WriteLine("Usando impresora de Windows como alternativa...");
                            WindowsBmpPrinter.Print(saved, string.IsNullOrWhiteSpace(windowsPrinterName) ? null : windowsPrinterName);
                            Console.WriteLine("Enviado a la impresora de Windows.");
                        }
                    }
                    catch (DllNotFoundException)
                    {
                        Console.WriteLine("Msprintsdk.dll no está disponible. Se omitió la impresión; el archivo BMP quedó guardado.");
                        // Fallback directo a impresora de Windows
                        try
                        {
                            Console.WriteLine("Usando impresora de Windows como alternativa...");
                            WindowsBmpPrinter.Print(saved, string.IsNullOrWhiteSpace(windowsPrinterName) ? null : windowsPrinterName);
                            Console.WriteLine("Enviado a la impresora de Windows.");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"Fallo impresión Windows: {ex2.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al imprimir: {ex.Message}. Se mantiene el archivo BMP.");
                        // Fallback a impresora de Windows
                        try
                        {
                            Console.WriteLine("Usando impresora de Windows como alternativa...");
                            WindowsBmpPrinter.Print(saved, string.IsNullOrWhiteSpace(windowsPrinterName) ? null : windowsPrinterName);
                            Console.WriteLine("Enviado a la impresora de Windows.");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"Fallo impresión Windows: {ex2.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando la factura: {ex.Message}");
            }

            Console.WriteLine("Presione ENTER para salir...");
            Console.ReadLine();
        }

        private static decimal ParseDecimal(string? input, CultureInfo culture)
        {
            if (decimal.TryParse(input, NumberStyles.Number, culture, out var v)) return v;
            // Intentar con cultura invariable en caso de que el usuario use punto
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out v)) return v;
            return 0m;
        }
    }
}
