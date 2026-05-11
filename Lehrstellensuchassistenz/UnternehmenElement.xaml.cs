using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
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
                    // Datei ist ungültig, Referenz löschen
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

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            string input = Company.Website?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return; // nichts tun, wenn leer

            string url = input;

            // Wenn kein http/https vorhanden, einfach voranstellen
            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + input;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
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

                    // 🖼 Bild anzeigen
                    PhotoPreview.Source = bitmap;
                    PhotoPlaceholderText.Visibility = Visibility.Collapsed;

                    // 💾 Pfad im Unternehmen speichern
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
