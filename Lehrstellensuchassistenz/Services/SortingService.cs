using System;
using System.Collections.Generic;
using System.Linq;
using Lehrstellensuchassistenz.Models;

namespace Lehrstellensuchassistenz.Services
{
    public class SortingService
    {
        /// <summary>
        /// Sortiert die Firmenliste. Zusagen stehen IMMER ganz oben.
        /// Danach folgen die Priorisierungen aus den Checkboxen und die Basissortierung.
        /// </summary>
        public List<Company> SortCompanies(IEnumerable<Company> companies, CompanyService.SortCriteria criteria)
        {
            if (companies == null) return new List<Company>();

            // 1. Absolute Priorität: Zusagen immer ganz nach oben
            var query = companies.OrderByDescending(c => c.Status == ApplicationStatus.Zusage);

            // 2. Sekundäre Priorität: Unbeworbene nach oben (wenn Checkbox aktiv)
            // Wir nutzen ThenByDescending, weil wir uns bereits in einer Query befinden
            query = query.ThenByDescending(c =>
                criteria.UnbeworbenOben && c.Status == ApplicationStatus.Unbeworben);

            // 3. "Nach unten"-Regeln
            // Prio: Abgelehnte nach unten
            query = query.ThenBy(c =>
                criteria.AbgelehntUnten && c.Status == ApplicationStatus.Abgelehnt);

            // Prio: Keine Antwort nach unten
            query = query.ThenBy(c =>
                criteria.KeineAntwortUnten && c.Status == ApplicationStatus.KeineAntwort);

            // 4. Die Hauptsortierung (Datum, Name, etc.)
            bool ascending = criteria.Tag.EndsWith("Asc");

            if (criteria.Tag.StartsWith("Date"))
            {
                query = ascending
                    ? query.ThenBy(c => c.CreatedAt)
                    : query.ThenByDescending(c => c.CreatedAt);
            }
            else if (criteria.Tag.StartsWith("Edit"))
            {
                query = ascending
                    ? query.ThenBy(c => c.LastModified)
                    : query.ThenByDescending(c => c.LastModified);
            }
            else
            {
                query = ascending
                    ? query.ThenBy(c => c.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    : query.ThenByDescending(c => c.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            }

            return query.ToList();
        }
    }
}