using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Lehrstellensuchassistenz.Models.Company;

namespace Lehrstellensuchassistenz.Views
{
    public partial class CompanyElement : Page
    {
        public Company Company { get; }
        private bool _isUpdating = false;

        // Instanziierung des neuen Services
        private readonly NotizenBoxImageInsertService _imageService = new NotizenBoxImageInsertService();

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

                if (Company != null)
                {
                    Company.UpdateTimestamp();
                }

                Dispatcher.BeginInvoke(new Action(() => { _isUpdating = false; }),
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };
        }

        public void SyncNotizenToModel()
        {
            if (NotizenBox == null || Company == null) return;
            TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);

            bool hasText = !string.IsNullOrWhiteSpace(range.Text) && range.Text != "\r\n";
            bool hasImages = NotizenBox.Document.Blocks.Any(b => b is Paragraph p && p.Inlines.Any(i => i is InlineUIContainer));

            if (!hasText && !hasImages)
            {
                if (Company.NotesXaml != null) Company.NotesXaml = null;
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                range.Save(ms, DataFormats.Rtf);
                Company.NotesXaml = Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public void LoadNotizen()
        {
            if (NotizenBox == null) return;

            _isUpdating = true;
            try
            {
                if (!string.IsNullOrEmpty(Company.NotesXaml))
                {
                    TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);
                    byte[] data = Encoding.UTF8.GetBytes(Company.NotesXaml);
                    using (MemoryStream ms = new MemoryStream(data)) { range.Load(ms, DataFormats.Rtf); }
                }
                else
                {
                    NotizenBox.Document.Blocks.Clear();
                }
            }
            catch { }
            finally
            {
                _isUpdating = false;
                UpdatePlaceholder();
            }
        }

        private void UpdatePlaceholder()
        {
            if (NotizenBox == null || PlaceholderText == null) return;
            TextRange range = new TextRange(NotizenBox.Document.ContentStart, NotizenBox.Document.ContentEnd);
            bool hasContent = (!string.IsNullOrWhiteSpace(range.Text) && range.Text != "\r\n")
                              || NotizenBox.Document.Blocks.Any(b => b is Paragraph p && p.Inlines.Any(i => i is InlineUIContainer));
            PlaceholderText.Visibility = hasContent ? Visibility.Collapsed : Visibility.Visible;
        }

        private void NotizenBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder();
            if (!_isUpdating) SyncNotizenToModel();
        }

        private void NotizenBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 1. Tab-Logik
            if (e.Key == Key.Tab)
            {
                EditingCommands.TabForward.Execute(null, NotizenBox);
                e.Handled = true;
                return;
            }

            // 2. Bild-Einfügen via Service
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Der Service prüft intern auf Clipboard.ContainsImage()
                if (_imageService.HandleImagePaste(NotizenBox, 500))
                {
                    e.Handled = true;
                }
            }
        }

        // ScaleLastInsertedImage wurde entfernt, da die Logik nun im Service liegt.

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Company.Website))
                BrowserService.OpenUrl(Company.Website);
        }

        private void Tipps_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                var type = btn.Tag.ToString() == "Bewerbungstipps" ? TippsType.Bewerbungstipps : TippsType.Lebenslauftipps;
                BrowserService.OpenTipps(type);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            SyncNotizenToModel();
            var templatesWin = new TemplatesWindow(Company, this);
            templatesWin.Owner = Application.Current.MainWindow;
            templatesWin.ShowDialog();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Company.LastApplicationPath))
                BrowserService.OpenUrl(Company.LastApplicationPath);
        }

        private void Select_Photo_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "Bilder (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dialog.ShowDialog() == true)
            {
                Company.PhotoReference = dialog.FileName;
                LoadPhotoPreview();
                Company.UpdateTimestamp();
            }
        }

        private void LoadPhotoPreview()
        {
            if (!string.IsNullOrEmpty(Company.PhotoReference) && File.Exists(Company.PhotoReference))
            {
                try
                {
                    PhotoPreview.Source = new BitmapImage(new Uri(Company.PhotoReference));
                    PhotoPlaceholderText.Visibility = Visibility.Collapsed;
                }
                catch { }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) => Keyboard.ClearFocus();
    }
}