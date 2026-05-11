using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Lehrstellensuchassistenz.Models;

namespace Lehrstellensuchassistenz.Services
{
    public class CompanyService
    {
        private readonly ObservableCollection<Company> _companies;
        // Wir holen uns den SortingService als Experten dazu
        private readonly SortingService _sortingService = new SortingService();

        public CompanyService(ObservableCollection<Company> companies)
        {
            _companies = companies ?? throw new ArgumentNullException(nameof(companies));
        }

        public void AddCompany(Company newCompany)
        {
            if (newCompany != null && !string.IsNullOrWhiteSpace(newCompany.Name))
            {
                _companies.Add(newCompany);
            }
        }

        public bool ConfirmAndDelete(Company? company)
        {
            if (company == null) return false;

            var result = MessageBox.Show(
                $"Soll die Firma \"{company.Name}\" wirklich gelöscht werden?",
                "Löschen bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                return _companies.Remove(company);
            }
            return false;
        }

        /// <summary>
        /// Delegiert die Sortierung an den SortingService.
        /// </summary>
        public List<Company> GetSortedList(SortCriteria criteria)
        {
            // Wir lassen den SortingService die Arbeit machen
            return _sortingService.SortCompanies(_companies, criteria);
        }

        // Die Definition bleibt hier, damit MainWindow.xaml.cs nicht kaputt geht
        public class SortCriteria
        {
            public string Tag { get; set; } = "DateDesc";
            public bool UnbeworbenOben { get; set; }
            public bool AbgelehntUnten { get; set; }
            public bool KeineAntwortUnten { get; set; }
        }
    }
}