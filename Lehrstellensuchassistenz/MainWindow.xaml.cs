using Lehrstellensuchassistenz;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IWshRuntimeLibrary;

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

            // ObservableCollection für Firmen initialisieren
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            Companies.CollectionChanged += (s, e) => SaveCompanies();
            LoadCompanies();
            MainFrame.Navigate(new CompanyListPage(Companies));

            // Shortcut prüfen und ggf. erstellen
            CheckDesktopShortcut();

            ShowWelcomePopup();
        }

        private const string RegistryKeyPath = @"SOFTWARE\Lehrstellensuchassistenz";
        private const string RegistryValueName = "DesktopShortcutAsked";

        private void CheckDesktopShortcut()
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            object? value = key?.GetValue(RegistryValueName);

            if (value == null) // Noch nie gefragt
            {
                var result = MessageBox.Show(
                    "Soll eine Verknüpfung auf dem Desktop erstellt werden?",
                    "Verknüpfung erstellen",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    CreateDesktopShortcut();
                    key?.SetValue(RegistryValueName, "Yes"); // gemerkt
                }
                else if (result == MessageBoxResult.No)
                {
                    key?.SetValue(RegistryValueName, "No"); // gemerkt
                }
                // Cancel → nichts speichern, nächstes Mal wieder fragen
            }
        }

        private const string WelcomeShownValue = "WelcomeShown";

        private void ShowWelcomePopup()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                object? value = key?.GetValue(WelcomeShownValue);

                if (value == null)
                {
                    // TextBlock für den Popup-Inhalt
                    var textBlock = new TextBlock
                    {
                        Text = @"Willkommen bei der Lehrstellensuchassistenz!
Oben findest du die wichtigsten Buttons:
- 'Zurück': Navigiert zur vorherigen Seite
- 'Alle Änderungen bisher speichern': Speichert alle aktuellen Änderungen
- 'Ausgewähltes Element löschen': Löscht die aktuell ausgewählte Firma

Rechts oben:
- 'Sortieren' ComboBox: Sortiert Firmen nach Name oder Datum
- 'Unternehmen hinzufügen': Öffnet das Formular, um eine neue Firma hinzuzufügen

Links in der Sidebar:
- 'Lebenslauf': Hier kannst du dein Lebenslauf-PDF hochladen und später öffnen
- 'Bewerbungen': Öffnet den Bewerbungsordner
- AMS, karriere.at, WKO, lehrstelle.at, lehrstellenportal.at, DevJobs: Öffnet die jeweiligen Webseiten
- 'Einstellungen': Platzhalter-Button für zukünftige Optionen

Im Hauptbereich:
- Du kannst alle deine gespeicherten Unternehmen bearbeiten

Die Ansicht kann mit Strg + Mausrad gezoomt werden.",
                        FontSize = 18,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20)
                    };

                    // OK-Button
                    var closeButton = new Button
                    {
                        Content = "OK",
                        Width = 120,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 15, 0, 15),
                        FontSize = 16,
                        FontWeight = FontWeights.SemiBold
                    };

                    // Popup-Fenster
                    var popup = new Window
                    {
                        Title = "Willkommen!",
                        SizeToContent = SizeToContent.Height, // Höhe passt sich Text + Button an
                        Width = 700,                          // feste Breite
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = this,
                        ResizeMode = ResizeMode.NoResize,
                        Content = new StackPanel
                        {
                            Children =
                    {
                        textBlock,
                        closeButton
                    }
                        }
                    };

                    closeButton.Click += (s, e) => popup.Close();

                    popup.ShowDialog();

                    key?.SetValue(WelcomeShownValue, "Yes");
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void CreateDesktopShortcut()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutLocation = Path.Combine(desktopPath, "Lehrstellensuchassistenz.lnk");

            string exePath = Process.GetCurrentProcess().MainModule!.FileName;

            // Pfad zum Icon in deinem Projekt-Ordner
            string projectFolder = Path.GetDirectoryName(exePath)!;
            string iconPath = Path.Combine(projectFolder, "resources", "images", "shortcut_icon.ico");

            if (!System.IO.File.Exists(iconPath))
            {
                MessageBox.Show("Icon-Datei nicht gefunden: " + iconPath);
                iconPath = exePath; // Fallback auf EXE-Icon
            }

            // WshShell Shortcut erstellen
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.Description = "Lehrstellensuchassistenz";
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.IconLocation = iconPath; // Eigenes Icon
            shortcut.Save();
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

        // Globale Variable in deiner MainWindow-Klasse
        private int zoomLevel = 0;       // Default 0
        private const int zoomMin = 0;   // 0 = Standardgröße
        private const int zoomMax = 4;   // 4 Schritte maximal

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0 && zoomLevel < zoomMax)
                {
                    zoomLevel++;
                    ZoomUI(1.1);  // 10% vergrößern
                }
                else if (e.Delta < 0 && zoomLevel > zoomMin)
                {
                    zoomLevel--;
                    ZoomUI(1 / 1.1);  // 10% verkleinern
                }

                e.Handled = true;
            }
        }

        private void ZoomUI(double zoomFactor)
        {
            // --- Top Bar ---
            TopBarGrid.Margin = new Thickness(
                TopBarGrid.Margin.Left * zoomFactor,
                TopBarGrid.Margin.Top * zoomFactor,
                TopBarGrid.Margin.Right * zoomFactor,
                TopBarGrid.Margin.Bottom * zoomFactor
            );

            foreach (var child in ((StackPanel)TopBarGrid.Children[0]).Children)
            {
                if (child is Button btn)
                {
                    btn.Width *= zoomFactor;
                    btn.Height *= zoomFactor;
                    btn.FontSize *= zoomFactor; // Text mitzoomen
                }
            }

            foreach (var child in ((StackPanel)TopBarGrid.Children[1]).Children)
            {
                if (child is Button btn)
                {
                    btn.Width *= zoomFactor;
                    btn.Height *= zoomFactor;
                    btn.FontSize *= zoomFactor;
                }
                else if (child is ComboBox cb)
                {
                    cb.Width *= zoomFactor;
                    cb.Height *= zoomFactor;
                    cb.FontSize *= zoomFactor; // Text in ComboBox
                }
            }

            // --- Sidebar ---
            SidebarScrollViewer.Width *= zoomFactor;

            if (SidebarScrollViewer.Content is StackPanel sidebarPanel)
            {
                foreach (var child in sidebarPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Width *= zoomFactor;
                        btn.Height *= zoomFactor;
                        btn.FontSize *= zoomFactor; // Text mitzoomen
                        btn.Margin = new Thickness(
                            btn.Margin.Left * zoomFactor,
                            btn.Margin.Top * zoomFactor,
                            btn.Margin.Right * zoomFactor,
                            btn.Margin.Bottom * zoomFactor
                        );
                    }
                }
            }
        }

        public void SaveCompanies()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            string json = JsonSerializer.Serialize(Companies, options);
            System.IO.File.WriteAllText(filePath, json);
        }
        private void LoadCompanies()
        {
            if (!System.IO.File.Exists(filePath))
            {
                // Datei leer anlegen, falls nicht vorhanden
                System.IO.File.WriteAllText(filePath, "[]");
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            string json = System.IO.File.ReadAllText(filePath);
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
            Unternehmen? companyToDelete = null;

            if (MainFrame.Content is CompanyListPage listPage && listPage.SelectedCompany != null)
            {
                // Fall 1: CompanyListPage
                companyToDelete = listPage.SelectedCompany;
            }
            else if (MainFrame.Content is UnternehmenElement detailPage && detailPage.Company != null)
            {
                // Fall 2: Detailansicht
                companyToDelete = detailPage.Company;
            }

            if (companyToDelete != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Soll die Firma \"{companyToDelete.Name}\" wirklich gelöscht werden?",
                    "Löschen bestätigen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    Companies.Remove(companyToDelete);

                    // Optional: Zurück zur Listenansicht navigieren
                    if (MainFrame.Content is UnternehmenElement)
                    {
                        MainFrame.Navigate(new CompanyListPage(Companies));
                    }
                }
            }
        }

        private void Sort_Click(object sender, RoutedEventArgs e) // NUR PLATZHALTER
        {
            MessageBox.Show("Button funktioniert!");
        }

        public enum WebSiteType
        {
            AMS,
            KarriereAt,
            WKO,
            LehrstelleAt,
            LehrstellenPortalAt,
            DevJobs
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button == null)
                return;

            // Basierend auf dem Content des Buttons eine Aktion ausführen
            WebSiteType websiteType = WebSiteType.AMS; // Default

            switch (button.Content.ToString())
            {
                case "AMS":
                    websiteType = WebSiteType.AMS;
                    break;
                case "karriere.at":
                    websiteType = WebSiteType.KarriereAt;
                    break;
                case "WKO":
                    websiteType = WebSiteType.WKO;
                    break;
                case "lehrstelle.at":
                    websiteType = WebSiteType.LehrstelleAt;
                    break;
                case "lehrstellenportal.at":
                    websiteType = WebSiteType.LehrstellenPortalAt;
                    break;
                case "DevJobs":
                    websiteType = WebSiteType.DevJobs;
                    break;
                default:
                    return;
            }

            // Jetzt kannst du das Enum `websiteType` verwenden, um die Seite zu öffnen
            OpenWebsite(websiteType);
        }

        // Diese Methode wird später verwendet, um die Seiten zu öffnen, basierend auf dem Enum
        private void OpenWebsite(WebSiteType site)
        {
            string url = string.Empty;

            switch (site)
            {
                case WebSiteType.AMS:
                    url = "https://www.ams.at";
                    break;
                case WebSiteType.KarriereAt:
                    url = "https://www.karriere.at";
                    break;
                case WebSiteType.WKO:
                    url = "https://lehrbetriebsuebersicht.wko.at/SearchLehrbetrieb.aspx";
                    break;
                case WebSiteType.LehrstelleAt:
                    url = "https://www.lehrstelle.at";
                    break;
                case WebSiteType.LehrstellenPortalAt:
                    url = "https://www.lehrstellenportal.at";
                    break;
                case WebSiteType.DevJobs:
                    url = "https://www.devjobs.at";
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void Lebenslauf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Bestimme den AppData-Pfad zum "user-files"-Ordner und "bewerbungen"-Ordner
                string userFilesOrdner = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Lehrstellensuchassistenz",
                    "user-files"
                );

                string bewerbungenOrdner = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Lehrstellensuchassistenz",
                    "bewerbungen"
                );

                // Den Dateipfad für das PDF (z.B. "Lebenslauf.pdf")
                string pdfDateiPfadUserFiles = Path.Combine(userFilesOrdner, "Lebenslauf.pdf");
                string pdfDateiPfadBewerbungen = Path.Combine(bewerbungenOrdner, "Lebenslauf.pdf");

                // Überprüfen, ob die Datei im "user-files"-Ordner existiert
                if (System.IO.File.Exists(pdfDateiPfadUserFiles))
                {
                    // Wenn die Datei existiert, kopiere sie auch in den Bewerbungsordner, falls sie noch nicht da ist
                    if (!System.IO.File.Exists(pdfDateiPfadBewerbungen))
                    {
                        // Datei in den Bewerbungsordner kopieren
                        System.IO.File.Copy(pdfDateiPfadUserFiles, pdfDateiPfadBewerbungen, true);
                    }

                    // Öffne das PDF aus dem "user-files"-Ordner
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pdfDateiPfadUserFiles,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Falls die Datei nicht im "user-files"-Ordner existiert, öffne den OpenFileDialog
                    OpenFileDialog dialog = new OpenFileDialog
                    {
                        Title = "Wählen Sie Ihren Lebenslauf aus",
                        Filter = "PDF-Dokumente (*.pdf)|*.pdf", // Nur PDF-Dateien
                        Multiselect = false // Keine Mehrfachauswahl
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        string ausgewaehlteDatei = dialog.FileName;

                        // Sicherstellen, dass beide Ordner existieren
                        Directory.CreateDirectory(userFilesOrdner);
                        Directory.CreateDirectory(bewerbungenOrdner);

                        try
                        {
                            // Kopiere die Datei in den "user-files"-Ordner
                            System.IO.File.Copy(ausgewaehlteDatei, pdfDateiPfadUserFiles, true); // Überschreibt die Datei, falls sie bereits existiert

                            // Kopiere die Datei auch in den Bewerbungsordner
                            System.IO.File.Copy(pdfDateiPfadUserFiles, pdfDateiPfadBewerbungen, true); // Überschreibt die Datei, falls sie bereits existiert

                            // Öffne das PDF
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = pdfDateiPfadUserFiles,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Fehler beim Speichern der Datei: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }

        // Sortieren nach Datum
        private void SortByDate(bool ascending = true)
        {
            var sorted = ascending
                ? Companies.OrderBy(c => c.ErstellDatum).ToList()
                : Companies.OrderByDescending(c => c.ErstellDatum).ToList();

            Companies.Clear();
            foreach (var c in sorted)
                Companies.Add(c);
        }

        // Sortieren nach Name (Anfangsbuchstabe)
        private void SortByName(bool ascending = true)
        {
            var sorted = ascending
                ? Companies.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList()
                : Companies.OrderByDescending(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();

            Companies.Clear();
            foreach (var c in sorted)
                Companies.Add(c);
        }

        // ComboBox SelectionChanged
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem item)
            {
                // Ignoriere den Platzhalter
                if (item.IsEnabled == false) return;

                string? tag = item.Tag?.ToString();
                bool ascending = tag?.EndsWith("Asc") == true;

                switch (tag)
                {
                    case "DateAsc":
                    case "DateDesc":
                        SortByDate(ascending);
                        break;

                    case "NameAsc":
                    case "NameDesc":
                        SortByName(ascending);
                        break;
                }

                // Platzhalter „Sortieren“ entfernen, damit es nicht mehr auswählbar ist
                if (SortComboBox.Items.Count > 0 && SortComboBox.Items[0] is ComboBoxItem placeholder && !placeholder.IsEnabled)
                {
                    SortComboBox.Items.RemoveAt(0);
                }

                // ComboBox neu auf ausgewähltes Item setzen (optional)
                // SortComboBox.SelectedItem = null;  // falls du möchtest, dass keine Auswahl sichtbar bleibt
            }
        }
        private void OpenBewerbungen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string bewerbungenOrdner = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Lehrstellensuchassistenz",
                    "bewerbungen"
                );

                // Ordner anlegen, falls er nicht existiert
                Directory.CreateDirectory(bewerbungenOrdner);

                // Explorer öffnen
                Process.Start(new ProcessStartInfo
                {
                    FileName = bewerbungenOrdner,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Öffnen des Bewerbungen-Ordners: " + ex.Message);
            }
        }
    }
}