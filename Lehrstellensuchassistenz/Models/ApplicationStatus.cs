using System.Collections.Generic;

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

    public static class StatusValues
    {
        public static List<ApplicationStatus> All => new List<ApplicationStatus>
        {
            ApplicationStatus.Unbeworben,
            ApplicationStatus.Beworben,
            ApplicationStatus.Eingeladen,
            ApplicationStatus.Zusage,
            ApplicationStatus.Abgelehnt,
            ApplicationStatus.KeineAntwort
        };
    }
}