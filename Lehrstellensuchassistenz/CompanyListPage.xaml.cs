using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Lehrstellensuchassistenz
{
    public partial class CompanyListPage : Page
    {
        public ObservableCollection<Unternehmen> Companies { get; }
        public Unternehmen? SelectedCompany => CompanyListBox.SelectedItem as Unternehmen;

        public CompanyListPage(ObservableCollection<Unternehmen> companies)
        {
            InitializeComponent();
            Companies = companies;
            CompanyListBox.ItemsSource = Companies;
        }
        private void CompanyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCompany != null)
            {
                NavigationService?.Navigate(new UnternehmenElement(SelectedCompany));
            }
        }
    }
}
