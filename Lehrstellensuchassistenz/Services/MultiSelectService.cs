using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Lehrstellensuchassistenz.Models;

namespace Lehrstellensuchassistenz.Services
{
    public class MultiSelectService
    {
        private readonly ObservableCollection<Company> _companies;

        public MultiSelectService(ObservableCollection<Company> companies)
        {
            _companies = companies;
        }

        /// <summary>
        /// Gibt alle aktuell markierten Unternehmen zurück.
        /// Benutzt jetzt "IsSelectedForAction" passend zum Model.
        /// </summary>
        public List<Company> GetSelectedCompanies()
        {
            // Hier war der Fehler: IsSelected -> IsSelectedForAction
            return _companies.Where(c => c.IsSelectedForAction).ToList();
        }

        /// <summary>
        /// Löscht alle markierten Unternehmen nach Bestätigung.
        /// </summary>
        public bool DeleteSelected()
        {
            var selected = GetSelectedCompanies();
            if (!selected.Any()) return false;

            var result = MessageBox.Show(
                $"{selected.Count} Unternehmen wirklich löschen?",
                "Massenlöschung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var company in selected)
                {
                    _companies.Remove(company);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ändert den Status für alle markierten Unternehmen.
        /// </summary>
        public void ChangeStatusForSelected(ApplicationStatus newStatus)
        {
            var selected = GetSelectedCompanies();
            foreach (var company in selected)
            {
                company.Status = newStatus;
                company.IsSelectedForAction = false; // Auswahl nach Aktion aufheben
            }
        }

        /// <summary>
        /// Setzt alle Checkboxen zurück (Deselect All).
        /// </summary>
        public void ClearSelection()
        {
            foreach (var company in _companies)
            {
                company.IsSelectedForAction = false;
            }
        }
    }
}