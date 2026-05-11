using System;
using System.Diagnostics;
using Lehrstellensuchassistenz.Models;

namespace Lehrstellensuchassistenz.Services
{
    public static class BrowserService
    {
        /// <summary>
        /// Öffnet eine beliebige URL im Standardbrowser.
        /// </summary>
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                // Sicherstellen, dass die URL ein Protokoll hat
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Da wir in einem Service sind, nutzen wir Debug anstatt direkt MessageBox, 
                // oder reichen den Fehler weiter.
                Debug.WriteLine("Link konnte nicht geöffnet werden: " + ex.Message);
            }
        }

        /// <summary>
        /// Öffnet spezifische Tipps basierend auf dem enum.
        /// </summary>
        // In der Datei BrowserService.cs

        public static void OpenTipps(Company.TippsType type)
        {
            string url = type == Company.TippsType.Bewerbungstipps
                ? "https://www.ams.at/arbeitsuchende/richtig-bewerben"
                : "https://www.karriere.at/c/lebenslauf";

            OpenUrl(url);
        }

        // Die Methode OpenSearchPortal brauchen wir nicht mehr zwingend, 
        // da die URLs jetzt direkt im MainWindow XAML stehen. 
        // Ich lasse sie als "Legacy" oder Sicherheit drin, falls du sie woanders nutzt.
        // Super, danke Gemini.
    }
}