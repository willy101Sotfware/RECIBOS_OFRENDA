using System;
using System.Configuration;
using System.IO;

namespace RECIBOS_OFRENDA
{
    internal static class AppConfig
    {
        public static string ImagesBasePath => Get("imagesBasePath", Path.Combine(AppContext.BaseDirectory, "Images", "Others"));
        public static string ThermalLogoFileName => Get("thermalLogoPath", "Vaucher2.png");
        public static bool UseWindowsPrinter => GetBool("useWindowsPrinter", true);

        // Render/Print tuning
        public static int PaperWidthPx => GetInt("paperWidthPx", 384); // 58mm: 384 px @203dpi. (80mm: 576)
        public static int MarginPx => GetInt("marginPx", 12);
        public static int PrintOffsetXPx => GetInt("printOffsetXPx", 0); // Desplazamiento manual horizontal (+ derecha / - izquierda)
        // Si usas los tamaños en puntos, serán ignorados si defines los tamaños en píxeles
        public static float HeaderFontSize => GetFloat("headerFontSize", 14.0f);
        public static float NormalFontSize => GetFloat("normalFontSize", 11.0f);
        public static float SmallFontSize => GetFloat("smallFontSize", 9.5f);

        // Tamaños de fuente en píxeles (toman prioridad si > 0)
        public static int HeaderFontPx => GetInt("headerFontPx", 20);
        public static int NormalFontPx => GetInt("normalFontPx", 14);
        public static int SmallFontPx  => GetInt("smallFontPx", 12);

        public static double LogoMaxPercent => GetDouble("logoMaxPercent", 0.35);
        public static double LogoMaxWidthMm => GetDouble("logoMaxWidthMm", 32.0);
        public static double LogoMaxHeightMm => GetDouble("logoMaxHeightMm", 16.0);

        public static string ThermalLogoFullPath
        {
            get
            {
                try
                {
                    var path = Path.Combine(ImagesBasePath, ThermalLogoFileName);
                    if (File.Exists(path)) return path;
                    var fallback = Path.Combine(AppContext.BaseDirectory, "Images", "Others", ThermalLogoFileName);
                    return fallback;
                }
                catch
                {
                    return ThermalLogoFileName;
                }
            }
        }

        private static string Get(string key, string defaultValue = "")
        {
            try
            {
                var v = ConfigurationManager.AppSettings[key];
                return string.IsNullOrWhiteSpace(v) ? defaultValue : v;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static int GetInt(string key, int defaultValue)
        {
            var s = Get(key, defaultValue.ToString());
            return int.TryParse(s, out var v) ? v : defaultValue;
        }

        private static float GetFloat(string key, float defaultValue)
        {
            var s = Get(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        private static double GetDouble(string key, double defaultValue)
        {
            var s = Get(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            var s = Get(key, defaultValue ? "true" : "false");
            return bool.TryParse(s, out var v) ? v : defaultValue;
        }
    }
}
