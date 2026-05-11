using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;

namespace Lehrstellensuchassistenz
{
    public partial class UnternehmenElement : Page
    {
        public Unternehmen Company { get; }

        private const string RegistryKeyPath = @"SOFTWARE\Lehrstellensuchassistenz";
        private const string WelcomeShownValue = "UnternehmenElementWelcomeShown";
        private bool _isInitializing = true;

        public UnternehmenElement(Unternehmen company)
        {
            InitializeComponent();
            isUpdating = true;

            Company = company;
            this.DataContext = Company;
            Loaded += UnternehmenElement_Loaded;

            ShowWelcomePopupIfNeeded();
            LoadPhoto();

            this.Unloaded += (s, e) => SaveBeforeLeave();

            // Ganz wichtig: Nach dem Initial-Load alles auf "sauber" setzen
            isUpdating = false;
            ResetDirtyFlag();
            _isInitializing = false;
        }

        // Diese Methode wird aufgerufen, wenn man von der Seite wegnavigiert
        public void SaveBeforeLeave()
        {
            // 1. Notizen aus der RichTextBox ins Model übertragen
            SyncNotizenToModel();

            // 2. Alles zentral speichern
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.SaveCompanies();
            }
        }

        private void UnternehmenElement_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNotizen();
        }

        private void ShowWelcomePopupIfNeeded()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                object? value = key?.GetValue(WelcomeShownValue);

                if (value == null)
                {
                    // TextBlock für Popup-Inhalt
                    var textBlock = new TextBlock
                    {
                        Text = @"Willkommen im Unternehmen-Detailbereich!

🔹 Oben links: Name, Link, Status sowie Datum und Uhrzeit der Erstellung des Elements

🔹 Oben rechts: Foto auswählen

🔹 Mitte: Notizen

🔹 Unten links: Wenn alle Felder au
sgefüllt sind, inklusive hochgeladenem Foto, erscheint der Button: Bewerben! / Schreiben fortsetzen

🔹 Unten rechts: Bewerbungstipps / Lebenslauftipps",
                        FontSize = 18,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20)
                    };

                    // OK-Button
                    var okButton = new Button
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
                        Width = 700,                     // feste Breite
                        SizeToContent = SizeToContent.Height, // Höhe passt sich Inhalt an
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = Window.GetWindow(this),
                        ResizeMode = ResizeMode.NoResize,
                        Content = new StackPanel
                        {
                            Children =
                    {
                        textBlock,
                        okButton
                    }
                        }
                    };

                    okButton.Click += (s, e) => popup.Close();

                    popup.ShowDialog();

                    // Wert speichern, dass Popup schon angezeigt wurde
                    key?.SetValue(WelcomeShownValue, "Yes");
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void LoadPhoto()
        {
            if (!string.IsNullOrEmpty(Company.FotoReferenz) && File.Exists(Company.FotoReferenz))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    using (var stream = File.OpenRead(Company.FotoReferenz))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze(); // Macht das Bild thread-sicher und performanter
                    PhotoPreview.Source = bitmap;
                    PhotoPlaceholderText.Visibility = Visibility.Collapsed;
                }
                catch { Company.FotoReferenz = null; }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Wir prüfen, ob die Quelle des Klicks das Grid selbst oder ein passives Element ist
            if (e.OriginalSource == sender || e.OriginalSource is Image || e.OriginalSource is StackPanel || e.OriginalSource is Border)
            {
                // 1. Fokus aus der aktuellen TextBox/RichTextBox entfernen
                Keyboard.ClearFocus();

                // 2. Den Fokus explizit auf das Hauptfenster setzen
                var mainWindow = Window.GetWindow(this);
                if (mainWindow != null)
                {
                    mainWindow.Focus();
                    // Optional: Den Fokus auf das Fenster-Element erzwingen
                    FocusManager.SetFocusedElement(mainWindow, mainWindow);
                }
            }
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
            Button? button = sender as Button;
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

                    MarkAsChanged(sender, e);

                    if (Application.Current.MainWindow is MainWindow main)
                    {
                        // WICHTIG: Erst Notizen sichern, dann die ganze Liste speichern
                        SyncNotizenToModel();
                        main.SaveCompanies();
                    }
                }
                catch
                {
                    MessageBox.Show("Ungültige oder beschädigte Bilddatei ❌");
                }
            }
        }

        private bool isUpdating = false;

        public void SyncNotizenToModel()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                if (DataContext is Unternehmen u)
                {
                    TextRange range = new TextRange(
                        NotizenBox.Document.ContentStart,
                        NotizenBox.Document.ContentEnd);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        range.Save(ms, DataFormats.Rtf);
                        u.NotizenXaml = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        public void LoadNotizen()
        {
            if (DataContext is Unternehmen u && !string.IsNullOrEmpty(u.NotizenXaml))
            {
                isUpdating = true;

                TextRange range = new TextRange(
                    NotizenBox.Document.ContentStart,
                    NotizenBox.Document.ContentEnd);

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(u.NotizenXaml)))
                {
                    range.Load(ms, DataFormats.Rtf);
                }

                isUpdating = false;
            }
        }

        private void NotizenBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    e.Handled = true;

                    // 1. Zeile DAVOR (erzwingen)
                    NotizenBox.CaretPosition.InsertLineBreak();
                    NotizenBox.CaretPosition = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward)
                                                ?? NotizenBox.CaretPosition;

                    // 2. BILD EINFÜGEN
                    NotizenBox.Paste();
                    ScaleLastInsertedImage(500);

                    // 3. ZEILE DANACH (Der entscheidende Teil)
                    // Wir holen uns die Position direkt NACH dem eingefügten Bild-Container
                    TextPointer nachBild = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);

                    if (nachBild != null)
                    {
                        // Cursor hinter das Bild setzen
                        NotizenBox.CaretPosition = nachBild;

                        // Jetzt den Umbruch machen
                        NotizenBox.CaretPosition.InsertLineBreak();

                        // Cursor in die neue leere Zeile setzen
                        NotizenBox.CaretPosition = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward)
                                                    ?? NotizenBox.Document.ContentEnd;
                    }
                    else
                    {
                        // Fallback, falls GetNextInsertionPosition fehlschlägt
                        NotizenBox.CaretPosition.InsertLineBreak();
                        NotizenBox.CaretPosition = NotizenBox.Document.ContentEnd;
                    }

                    NotizenBox.Focus();
                }
            }
        }

        private void ScaleLastInsertedImage(double maxWidth)
        {
            // Dokument von hinten durchsuchen
            foreach (var block in NotizenBox.Document.Blocks.Reverse())
            {
                if (block is Paragraph p)
                {
                    foreach (var inline in p.Inlines.Reverse())
                    {
                        if (inline is InlineUIContainer container && container.Child is Image img)
                        {
                            // 1. Breite limitieren
                            img.Width = maxWidth;

                            // 2. WICHTIG: Höhe auf Auto setzen, damit kein leerer Platz bleibt
                            img.Height = double.NaN;

                            // 3. Stretch-Modus sicherstellen
                            img.Stretch = Stretch.Uniform;

                            // 4. Den Container zwingen, sich an das Bild anzupassen
                            container.BaselineAlignment = BaselineAlignment.Bottom;

                            // Layout-Update für dieses spezifische Bild erzwingen
                            img.UpdateLayout();
                            return;
                        }
                    }
                }
            }
        }

        public bool IsDirty { get; private set; } = false;
        private void MarkAsChanged(object sender, EventArgs e)
        {
            if (isUpdating || _isInitializing) return;

            // Nur markieren, wenn der Nutzer wirklich gerade mit dem Element interagiert
            if (sender is Control c && (c.IsFocused || c.IsKeyboardFocusWithin))
            {
                this.IsDirty = true;
                Company.ZuletztGeaendert = DateTime.Now; // Hier direkt den Zeitstempel setzen!
            }
        }

        public void ResetDirtyFlag()
        {
            IsDirty = false;
        }

        private void NotizenBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 1. UI Logik (Placeholder) immer ausführen
            var text = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd).Text;
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Visible : Visibility.Hidden;

            // 2. Dirty-Logik nur, wenn wir NICHT laden UND der User wirklich drin tippt
            if (!isUpdating && !_isInitializing && NotizenBox.IsFocused)
            {
                MarkAsChanged(sender, e);
            }
        }
    }
}