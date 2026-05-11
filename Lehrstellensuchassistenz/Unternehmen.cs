using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lehrstellensuchassistenz
{
    public class Unternehmen
    {
        public string? Name { get; set; }
        public string? Website { get; set; }
        public Status BewerbungsStatus { get; set; }
        public string? Notizen { get; set; }
        public string? FotoReferenz { get; set; }
    }

    public enum Status
    {
        Unbeworben,
        Beworben,
        KeineAntwort,
        Abgelehnt,
        NächsteSchritte,
        Praktikum
    }
}
