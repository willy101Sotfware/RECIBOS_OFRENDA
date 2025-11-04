using System;
using System.Configuration;
using System.IO;

namespace RECIBOS_OFRENDA
{
    internal static class AppConfig
    {
        public static string ImagesBasePath => Get("imagesBasePath", Path.Combine(AppContext.BaseDirectory, "Images", "Others"));
        public static string ThermalLogoFileName => Get("thermalLogoPath", "Vaucher2.png");

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
    }
}
