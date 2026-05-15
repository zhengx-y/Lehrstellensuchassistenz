using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Resources.Languages; // Wichtig für den Zugriff auf Langs

namespace Lehrstellensuchassistenz.Services
{
    public class CompanyService
    {
        private readonly ObservableCollection<Company> _companies;
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

            // Nutzt jetzt MsgConfirmDeleteSingle und MsgConfirmDeleteTitle aus Langs
            var result = MessageBox.Show(
                $"{Langs.MsgConfirmDeleteSingle}\n\n({company.Name})",
                Langs.MsgConfirmDeleteTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                return _companies.Remove(company);
            }
            return false;
        }

        public List<Company> GetSortedList(SortCriteria criteria)
        {
            return _sortingService.SortCompanies(_companies, criteria);
        }

        public class SortCriteria
        {
            public string Tag { get; set; } = "DateDesc";
            public bool UnbeworbenOben { get; set; }
            public bool AbgelehntUnten { get; set; }
            public bool KeineAntwortUnten { get; set; }
        }
    }
}