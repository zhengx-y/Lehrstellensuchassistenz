using Lehrstellensuchassistenz;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;

namespace Lehrstellensuchassistenz
{
    public partial class MainWindow : Window
    {
        // zentrale Datenquelle für alle Pages
        public ObservableCollection<Unternehmen> Companies { get; } = new ObservableCollection<Unternehmen>();
        private readonly string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz", "companies.json");

        public MainWindow()
        {
            InitializeComponent();

            // Ordner sicherstellen
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // ObservableCollection automatisch speichern, wenn sich etwas ändert
            Companies.CollectionChanged += (s, e) => SaveCompanies();

            LoadCompanies();
            MainFrame.Navigate(new CompanyListPage(Companies));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Strg+S
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveCompanies();
                MessageBox.Show("Alle Änderungen wurden gespeichert!");
                e.Handled = true;
            }

            // Delete-Taste
            if (e.Key == Key.Delete)
            {
                Delete_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void SaveCompanies()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            string json = JsonSerializer.Serialize(Companies, options);
            File.WriteAllText(filePath, json);
        }
        private void LoadCompanies()
        {
            if (!File.Exists(filePath))
            {
                // Datei leer anlegen, falls nicht vorhanden
                File.WriteAllText(filePath, "[]");
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            string json = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<ObservableCollection<Unternehmen>>(json, options);

            if (loaded != null)
            {
                foreach (var c in loaded)
                    Companies.Add(c);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveCompanies();
            base.OnClosing(e);
        }

        private void ReturnPage_Click(object sender, RoutedEventArgs e)
        {
            SaveCompanies();
            // Navigiert zurück zur CompanyListPage und übergibt weiterhin die zentrale ObservableCollection
            MainFrame.Navigate(new CompanyListPage(Companies));
        }

        private void SaveCompanies_Click(object sender, RoutedEventArgs e)
        {
            SaveCompanies();
            MessageBox.Show("Alle Änderungen wurden gespeichert!");
        }

        private void AddCompany_Click(object sender, RoutedEventArgs e)
        {
            UnternehmenHinzufuegen unternehmenHinzufuegen = new UnternehmenHinzufuegen();
            unternehmenHinzufuegen.Owner = this;

            bool? result = unternehmenHinzufuegen.ShowDialog();

            if (result == true)
            {
                Unternehmen? company = unternehmenHinzufuegen.Answer;

                if (company != null && !string.IsNullOrWhiteSpace(company.Name))
                {
                    Companies.Add(company);
                }
                else
                {
                    MessageBox.Show("Die Firma hat keinen gültigen Namen.");
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is CompanyListPage page && page.SelectedCompany != null)
            {
                var company = page.SelectedCompany;
                MessageBoxResult result = MessageBox.Show(
                    $"Soll die Firma \"{company.Name}\" wirklich gelöscht werden?",
                    "Löschen bestätigen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                if (result == MessageBoxResult.Yes)
                {
                    Companies.Remove(company);
                }
            }
        }
        
        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button funktioniert!");
        }
    }
}
