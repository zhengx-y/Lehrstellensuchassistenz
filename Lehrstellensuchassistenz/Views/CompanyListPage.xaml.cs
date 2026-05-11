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
            if (CompanyListBox != null)
            {
                // Items.Refresh() ist hier meistens gar nicht mehr nötig, 
                // da ObservableCollection selbst Events feuert. 
                // Aber es schadet nicht, um die Sortierung zu erzwingen.
                CompanyListBox.Items.Refresh();
            }
        }

        private void CompanyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCompany != null && Application.Current.MainWindow is MainWindow main)
            {
                // Zugriff auf den jetzt öffentlichen NavigationService
                main.NavigationService.NavigateTo(new CompanyElement(SelectedCompany));
            }
        }
    }
}