using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Lehrstellensuchassistenz.Services.CompanyService;

namespace Lehrstellensuchassistenz.Views
{
    public partial class CompanyListPage : Page
    {
        private readonly ObservableCollection<Company> _allCompanies;

        public Company? SelectedCompany => CompanyListBox.SelectedItem as Company;

        public CompanyListPage(ObservableCollection<Company> companies)
        {
            InitializeComponent();
            _allCompanies = companies;

            // WICHTIG: Hier die Verbindung herstellen!
            CompanyListBox.ItemsSource = _allCompanies;

            // Initiales Refresh (optional, aber schadet nicht)
            RefreshList();
        }

        /// <summary>
        /// Aktualisiert die Anzeige basierend auf den Sortier-Kriterien.
        /// </summary>
        public void RefreshList()
        {
            // Das erzwingt das Neuzeichnen der Element-Container
            CompanyListBox.Items.Refresh();
        }

        private void CompanyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCompany != null && Application.Current.MainWindow is MainWindow main)
            {
                // Zugriff auf den jetzt öffentlichen NavigationService
                main.NavigationService.NavigateTo(new CompanyElement(SelectedCompany));
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // e.RemovedItems.Count > 0 stellt sicher, dass vorher ein Wert da war 
            // und nicht gerade die ganze Liste gelöscht wurde.
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.SaveAllData();
                    main.ApplySorting();
                }
            }
        }
    }
}