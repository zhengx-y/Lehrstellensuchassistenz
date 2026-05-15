using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Resources.Languages; // Für potenzielle Fehlermeldungen

namespace Lehrstellensuchassistenz.Services
{
    public static class BrowserService
    {
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Hier könnte man Langs.MsgInfo nutzen, falls man eine MessageBox will
                Debug.WriteLine($"{Langs.ErrLinkOpened}: {ex.Message}");
            }
        }

        public static void OpenTipps(Company.TippsType type)
        {
            // Wir prüfen die aktuelle Sprache der App
            bool isEnglish = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);
            string url;

            if (type == Company.TippsType.Bewerbungstipps)
            {
                // Bewerbungstipps: AMS für AT/DE, Prospects für EN
                url = isEnglish
                    ? "https://www.prospects.ac.uk/careers-advice/applying-for-jobs"
                    : "https://www.ams.at/arbeitsuchende/richtig-bewerben";
            }
            else // CV Tipps
            {
                // Lebenslauftipps: karriere.at für AT, Monster für EN
                url = isEnglish
                    ? "https://www.monster.com/career-advice/article/cv-tips"
                    : "https://www.karriere.at/c/lebenslauf";
            }

            OpenUrl(url);
        }
    }
}