using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Lehrstellensuchassistenz
{
    public partial class UnternehmenElement : Page
    {
        public Unternehmen Company { get; }

        public UnternehmenElement(Unternehmen company)
        {
            InitializeComponent();
            Company = company;
            this.DataContext = Company;

            // 🔄 Bild laden wenn vorhanden
            if (!string.IsNullOrEmpty(Company.FotoReferenz) && File.Exists(Company.FotoReferenz))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Company.FotoReferenz);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    PhotoPreview.Source = bitmap;
                    PhotoPlaceholderText.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    Company.FotoReferenz = null;
                }
            }

            Company.PropertyChanged += (s, e) =>
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.SaveCompanies();
                }
            };
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button funktioniert!");
        }

        // Enum für die verschiedenen Typen der Tipps
        public enum TippsType
        {
            Bewerbungstipps,
            Lebenslauftipps
        }

        private void Tipps_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            // Basierend auf dem Content des Buttons den entsprechenden Typ festlegen
            TippsType tippsType = TippsType.Bewerbungstipps; // Default

            switch (button.Content.ToString())
            {
                case "Bewerbungstipps":
                    tippsType = TippsType.Bewerbungstipps;
                    break;
                case "Lebenslauftipps":
                    tippsType = TippsType.Lebenslauftipps;
                    break;
                default:
                    return;
            }

            // Jetzt kannst du den Typ `tippsType` verwenden, um die passenden Tipps anzuzeigen
            ShowTipps(tippsType);
        }

        // Methode, um die Tipps basierend auf dem Typ zu zeigen
        private void ShowTipps(TippsType tipps)
        {
            string url = string.Empty;

            switch (tipps)
            {
                case TippsType.Bewerbungstipps:
                    url = "https://www.ams.at/arbeitsuchende/richtig-bewerben";
                    break;
                case TippsType.Lebenslauftipps:
                    url = "https://www.ams.at/arbeitsuchende/richtig-bewerben/ansprechender-lebenslauf";
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        // 🔹 Bewerben-Button
        private void Bewerben_Click(object sender, RoutedEventArgs e)
        {
            var fenster = new VorlagenWindow(Company, this); // Übergabe von 'this' (UnternehmenElement)
            fenster.Owner = Window.GetWindow(this);

            if (fenster.ShowDialog() == true && !string.IsNullOrEmpty(fenster.GewaehlteDatei))
            {
                Company.LetzteBewerbungPfad = fenster.GewaehlteDatei;
            }
        }

        // 🔹 Weiterschreiben-Button
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Überprüfe, ob der Pfad existiert und die Datei tatsächlich da ist
            if (!string.IsNullOrEmpty(Company.LetzteBewerbungPfad) && File.Exists(Company.LetzteBewerbungPfad))
            {
                // Überprüfen, ob es sich um eine Word-Datei handelt
                string fileExtension = Path.GetExtension(Company.LetzteBewerbungPfad).ToLower();
                if (fileExtension == ".docx" || fileExtension == ".doc")
                {
                    try
                    {
                        // Versuchen, das Dokument zu öffnen
                        Process.Start(new ProcessStartInfo(Company.LetzteBewerbungPfad)
                        {
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        // Wenn ein Fehler auftritt (z.B. Datei nicht mehr vorhanden), informiere den Benutzer
                        MessageBox.Show($"Es gab ein Problem beim Öffnen der Datei: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Die Datei ist kein unterstütztes Word-Dokument. Bitte wählen Sie eine .docx oder .doc Datei.");
                }
            }
            else
            {
                // Wenn die Datei nicht existiert oder der Pfad leer ist
                MessageBox.Show("Keine vorherige Bewerbung zum Weiterschreiben gefunden.");
                ContinueButton.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowContinueButton()
        {
            ContinueButton.Visibility = Visibility.Visible;
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            string? input = Company.Website?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return;

            string url = input;
            if (!input.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + input;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void Select_Photo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Bild auswählen",
                Filter = "Bilddateien (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    PhotoPreview.Source = bitmap;
                    PhotoPlaceholderText.Visibility = Visibility.Collapsed;

                    Company.FotoReferenz = dialog.FileName;

                    if (Application.Current.MainWindow is MainWindow main)
                    {
                        main.SaveCompanies();
                    }
                }
                catch
                {
                    MessageBox.Show("Ungültige oder beschädigte Bilddatei ❌");
                }
            }
        }
    }
}