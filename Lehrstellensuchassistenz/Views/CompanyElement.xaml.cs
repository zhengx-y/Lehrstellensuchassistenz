using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq; // Für Reverse() wichtig
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media; // Für Stretch wichtig
using System.Windows.Media.Imaging;

namespace Lehrstellensuchassistenz.Views
{
    public partial class CompanyElement : Page
    {
        public Company Company { get; }
        private bool _isUpdating = false;

        public CompanyElement(Company company)
        {
            InitializeComponent();
            Company = company;
            this.DataContext = Company;

            this.Loaded += (s, e) => {
                _isUpdating = true;
                LoadNotizen();
                LoadPhotoPreview();
                UpdatePlaceholder();

                // FIX: Wir zwingen die UI, CanApply sofort neu zu berechnen.
                // Das ist wichtig, falls die Firma gerade erst erstellt wurde.
                if (Company != null)
                {
                    // Wir triggern das PropertyChanged-Event für CanApply manuell an
                    Company.UpdateTimestamp();
                }

                Dispatcher.BeginInvoke(new Action(() => { _isUpdating = false; }),
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };
        }

        public void SaveBeforeLeave()
        {
            bool wasUpdating = _isUpdating;
            _isUpdating = false;
            SyncNotizenToModel();
            _isUpdating = wasUpdating;

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.SaveAllData();
            }
        }

        private void UpdatePlaceholder()
        {
            if (NotizenBox == null || PlaceholderText == null) return;
            TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);

            // Check auf echten Inhalt (Bilder zählen als Inhalt, auch wenn Text leer ist!)
            bool hasContent = !string.IsNullOrWhiteSpace(range.Text) && range.Text != "\r\n"
                              || NotizenBox.Document.Blocks.Any(b => b is Paragraph p && p.Inlines.Any(i => i is InlineUIContainer));

            PlaceholderText.Visibility = hasContent ? Visibility.Collapsed : Visibility.Visible;
        }

        private void NotizenBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder();

            if (!_isUpdating)
            {
                // Wir erzwingen die Synchronisation bei jeder Änderung
                SyncNotizenToModel();

                // MarkAsChanged/Timestamp wird durch den Setter von NotesXaml im Model getriggert
            }
        }

        private void MarkAsChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            Company.UpdateTimestamp();
        }

        // --- BILDER & KEY LOGIK ---

        private void NotizenBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 1. Tab-Logik
            if (e.Key == Key.Tab)
            {
                EditingCommands.TabForward.Execute(null, NotizenBox);
                e.Handled = true;
                return;
            }

            // 2. Bild-Einfügen Logik (Strg + V)
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    e.Handled = true;

                    // Umbruch davor
                    NotizenBox.CaretPosition.InsertLineBreak();
                    NotizenBox.CaretPosition = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward) ?? NotizenBox.CaretPosition;

                    // Bild einfügen
                    NotizenBox.Paste();
                    ScaleLastInsertedImage(500);

                    // Umbruch danach
                    TextPointer nachBild = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
                    if (nachBild != null)
                    {
                        NotizenBox.CaretPosition = nachBild;
                        NotizenBox.CaretPosition.InsertLineBreak();
                        NotizenBox.CaretPosition = NotizenBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward) ?? NotizenBox.Document.ContentEnd;
                    }
                    else
                    {
                        NotizenBox.CaretPosition.InsertLineBreak();
                        NotizenBox.CaretPosition = NotizenBox.Document.ContentEnd;
                    }

                    NotizenBox.Focus();
                }
            }
        }

        private void ScaleLastInsertedImage(double maxWidth)
        {
            foreach (var block in NotizenBox.Document.Blocks.Reverse())
            {
                if (block is Paragraph p)
                {
                    foreach (var inline in p.Inlines.Reverse())
                    {
                        if (inline is InlineUIContainer container && container.Child is Image img)
                        {
                            img.Width = maxWidth;
                            img.Height = double.NaN; // Auto-Höhe
                            img.Stretch = Stretch.Uniform;
                            container.BaselineAlignment = BaselineAlignment.Bottom;
                            img.UpdateLayout();
                            return;
                        }
                    }
                }
            }
        }

        // --- Restliche Methoden (Unverändert) ---

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Company.Website)) BrowserService.OpenUrl(Company.Website);
        }

        private void Tipps_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var type = btn.Tag?.ToString() == "Bewerbungstipps" ? Company.TippsType.Bewerbungstipps : Company.TippsType.Lebenslauftipps;
                BrowserService.OpenTipps(type);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            var templatesWin = new TemplatesWindow(Company, this);
            templatesWin.Owner = Application.Current.MainWindow;
            templatesWin.ShowDialog();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Company.LastApplicationPath))
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Company.LastApplicationPath) { UseShellExecute = true }); }
                catch (Exception ex) { MessageBox.Show("Datei konnte nicht geöffnet werden: " + ex.Message); }
            }
        }

        private void Select_Photo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "Bilder (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dialog.ShowDialog() == true)
            {
                Company.PhotoReference = dialog.FileName;
                LoadPhotoPreview();
            }
        }

        private void LoadPhotoPreview()
        {
            if (!string.IsNullOrEmpty(Company.PhotoReference) && File.Exists(Company.PhotoReference))
            {
                try { PhotoPreview.Source = new BitmapImage(new Uri(Company.PhotoReference)); PhotoPlaceholderText.Visibility = Visibility.Collapsed; }
                catch { }
            }
        }

        public void SyncNotizenToModel()
        {
            if (NotizenBox == null || Company == null) return;

            TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);

            // Check: Ist wirklich Text oder ein Bild vorhanden?
            // range.Text enthält bei einer leeren RichTextBox oft "\r\n", daher das Trim().
            bool hasText = !string.IsNullOrWhiteSpace(range.Text);
            bool hasImages = NotizenBox.Document.Blocks.Any(b => b is Paragraph p && p.Inlines.Any(i => i is InlineUIContainer));

            if (!hasText && !hasImages)
            {
                // Box ist wirklich leer -> Model auf null setzen
                if (Company.NotesXaml != null)
                {
                    Company.NotesXaml = null;
                }
                return;
            }

            // Wenn Inhalt da ist, als RTF speichern
            using (MemoryStream ms = new MemoryStream())
            {
                range.Save(ms, DataFormats.Rtf);
                string newRtf = Encoding.UTF8.GetString(ms.ToArray());

                // Nur updaten, wenn der RTF-Code sich wirklich unterscheidet
                if (Company.NotesXaml != newRtf)
                {
                    Company.NotesXaml = newRtf;
                }
            }
        }

        public void LoadNotizen()
        {
            if (string.IsNullOrEmpty(Company.NotesXaml) || NotizenBox == null) return;
            _isUpdating = true;
            try
            {
                TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);
                byte[] data = Encoding.UTF8.GetBytes(Company.NotesXaml);
                using (MemoryStream ms = new MemoryStream(data)) { range.Load(ms, DataFormats.Rtf); }
            }
            catch { try { NotizenBox.AppendText(Company.NotesXaml); } catch { } }
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}