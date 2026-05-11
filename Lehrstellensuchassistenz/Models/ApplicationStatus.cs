using System.ComponentModel;

namespace Lehrstellensuchassistenz.Models
{
    public enum ApplicationStatus
    {
        [Description("Noch nicht beworben")] Unbeworben,
        [Description("Beworben")] Beworben,
        [Description("Keine Antwort")] KeineAntwort,
        [Description("Eingeladen")] Eingeladen,
        [Description("Abgelehnt")] Abgelehnt,
        [Description("Zusage")] Zusage
    }
}