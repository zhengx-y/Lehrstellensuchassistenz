using Lehrstellensuchassistenz.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Bindung der ListBox an die Datenquelle
            CompanyListBox.ItemsSource = _allCompanies;
        }

        /// <summary>
        /// Erzwingt eine visuelle Aktualisierung der Liste.
        /// </summary>
        public void RefreshList()
        {
            if (CompanyListBox.Items != null)
            {
                CompanyListBox.Items.Refresh();
            }
        }

        private void CompanyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCompany != null && Application.Current.MainWindow is MainWindow main)
            {
                main.NavigationService.NavigateTo(new CompanyElement(SelectedCompany));
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Triggert nur bei Benutzerinteraktion
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.SaveAllData();
                    main.ApplySorting();

                    // Wichtig: Auch hier prüfen, ob die Leiste ein/ausgeblendet werden muss
                    main.RefreshMultiSelectVisibility();
                }
            }
        }

        /// <summary>
        /// DIESER NAME MUSS MIT DEM XAML (Click="SelectionCheckBox_Click") ÜBEREINSTIMMEN
        /// </summary>
        private void SelectionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mw)
            {
                // Aktualisiert sofort die Sichtbarkeit der Mass-Select-Leiste im MainWindow
                mw.RefreshMultiSelectVisibility();
            }
        }
    }
}