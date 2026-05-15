using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Resources.Languages; // Wichtig für Langs

namespace Lehrstellensuchassistenz.Services
{
    public class MultiSelectService
    {
        private readonly ObservableCollection<Company> _companies;

        public MultiSelectService(ObservableCollection<Company> companies)
        {
            _companies = companies;
        }

        public List<Company> GetSelectedCompanies()
        {
            return _companies.Where(c => c.IsSelectedForAction).ToList();
        }

        /// <summary>
        /// Löscht alle markierten Unternehmen mit lokalisierten Texten.
        /// </summary>
        public bool DeleteSelected()
        {
            var selected = GetSelectedCompanies();
            if (!selected.Any()) return false;

            // Nutzt jetzt MsgConfirmDeleteBulk und MsgConfirmDeleteTitle aus Langs
            var result = MessageBox.Show(
                $"{selected.Count} {Langs.MsgConfirmDeleteBulk}",
                Langs.MsgConfirmDeleteTitle,
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

        public void ChangeStatusForSelected(ApplicationStatus newStatus)
        {
            var selected = GetSelectedCompanies();
            foreach (var company in selected)
            {
                company.Status = newStatus;
                company.IsSelectedForAction = false;
            }
        }

        public void ClearSelection()
        {
            foreach (var company in _companies)
            {
                company.IsSelectedForAction = false;
            }
        }
    }
}