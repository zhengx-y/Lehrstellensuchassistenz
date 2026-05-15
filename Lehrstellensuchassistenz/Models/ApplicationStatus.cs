using System.ComponentModel;

namespace Lehrstellensuchassistenz.Models
{
    public enum ApplicationStatus
    {
        Unbeworben,
        Beworben,
        KeineAntwort,
        Eingeladen,
        Abgelehnt,
        Zusage
    }

    public static class ApplicationStatusExtensions
    {
        public static string ToDisplayName(this ApplicationStatus status)
        {
            // Holt den Text direkt aus der Langs.resx (und damit automatisch die richtige Sprache)
            return Lehrstellensuchassistenz.Resources.Languages.Langs.ResourceManager.GetString("Status" + status.ToString()) ?? status.ToString();
        }
    }
}