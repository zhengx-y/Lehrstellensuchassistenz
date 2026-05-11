using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Lehrstellensuchassistenz.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lehrstellensuchassistenz
{
    public partial class MainWindow : Window
    {
        // Zentrale Datenliste
        public ObservableCollection<Company> Companies { get; } = new ObservableCollection<Company>();

        private readonly FileService _fileService = new FileService();
        public readonly CompanyService CompanyService;
        public readonly NavigationService NavigationService;
        private readonly UIService _uiService = new UIService();

        // NEU: ShortcutService Instanz
        private ShortcutService _shortcutService;

        public MainWindow()
        {
            InitializeComponent();

            // Initialisierung der Services
            CompanyService = new CompanyService(Companies);
            NavigationService = new NavigationService(MainFrame);

            // NEU: ShortcutService initialisieren
            _shortcutService = new ShortcutService(this);

            LoadAllData();

            // Startseite: Die Liste aller Firmen anzeigen
            NavigationService.NavigateTo(new CompanyListPage(Companies));

            // NEU: Wir abonnieren das Loaded-Event, um Popups nach dem Rendern anzuzeigen
            this.Loaded += MainWindow_Loaded;
        }

        // NEU: Hier werden die Popups beim Start getriggert
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Prüfen ob Verknüpfung erstellt werden soll
            _shortcutService.CheckDesktopShortcut();

            // 2. Willkommens-Anleitung zeigen
            _shortcutService.ShowWelcomePopup();
        }

        #region Speichern & Laden
        // ... (Dein restlicher Code bleibt gleich) ...
        private void LoadAllData()
        {
            var loaded = _fileService.Load();
            Companies.Clear();
            foreach (var c in loaded) Companies.Add(c);
            ApplySorting();
        }

        public void SaveAllData()
        {
            if (MainFrame.Content is CompanyElement detailPage)
            {
                detailPage.SyncNotizenToModel();
            }

            _fileService.Save(Companies);
        }

        private void SaveAllData_Click(object sender, RoutedEventArgs e)
        {
            SaveAllData();
            MessageBox.Show("Alle Änderungen wurden sicher gespeichert!", "Speichern", MessageBoxButton.OK, MessageBoxImage.Information);
            ApplySorting();
        }
        #endregion

        #region Hotkeys & UI-Zoom
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                SaveAllData();
                ApplySorting();
                e.Handled = true;
            }

            if (e.Key == Key.Delete && !IsUserTyping())
            {
                DeleteCompany_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }

            if (e.Key == Key.Escape)
            {
                if (MainFrame.Content is CompanyElement)
                {
                    NavigateBack_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double? factor = _uiService.GetZoomFactor(e.Delta);
                if (factor.HasValue) ApplyZoom(factor.Value);
                e.Handled = true;
            }
        }

        private void ApplyZoom(double factor)
        {
            TopBarGrid.Margin = ScaleThickness(TopBarGrid.Margin, factor);
            ScaleChildren(TopBarGrid, factor);
            ScaleSidebar(factor);
        }

        private Thickness ScaleThickness(Thickness old, double factor) =>
            new Thickness(old.Left * factor, old.Top * factor, old.Right * factor, old.Bottom * factor);

        private void ScaleChildren(Panel parent, double factor)
        {
            foreach (UIElement child in parent.Children)
            {
                if (child is FrameworkElement fe)
                {
                    if (!double.IsNaN(fe.Width)) fe.Width *= factor;
                    if (!double.IsNaN(fe.Height)) fe.Height *= factor;
                    fe.Margin = ScaleThickness(fe.Margin, factor);
                    if (fe is Control ctrl) ctrl.FontSize *= factor;
                    if (fe is TextBlock tb) tb.FontSize *= factor;
                }
            }
        }

        private void ScaleSidebar(double factor)
        {
            SidebarGrid.Width *= factor;
            ScaleChildren(SidebarGrid, factor);
        }
        #endregion

        #region Logik & Navigation
        public void DeleteCompany_Click(object sender, RoutedEventArgs e)
        {
            Company? toDelete = null;
            if (MainFrame.Content is CompanyListPage listPage) toDelete = listPage.SelectedCompany;
            else if (MainFrame.Content is CompanyElement detailPage) toDelete = detailPage.Company;

            if (toDelete != null && CompanyService.ConfirmAndDelete(toDelete))
            {
                if (MainFrame.Content is CompanyElement)
                {
                    NavigationService.NavigateTo(new CompanyListPage(Companies));
                }
                SaveAllData();
                ApplySorting();
            }
        }

        private bool IsUserTyping()
        {
            var focused = FocusManager.GetFocusedElement(this);
            return focused is TextBox || focused is RichTextBox;
        }

        private void Quicklink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string url)
            {
                BrowserService.OpenUrl(url);
            }
        }

        private void Resume_Click(object sender, RoutedEventArgs e) => _fileService.OpenOrSelectResume();
        private void OpenApplicationsFolder_Click(object sender, RoutedEventArgs e) => _fileService.OpenBewerbungenFolder();

        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            SaveAllData();
            NavigationService.NavigateTo(new CompanyListPage(Companies));
        }

        private void AddCompany_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddCompany { Owner = this };
            if (dialog.ShowDialog() == true && dialog.Answer != null)
            {
                CompanyService.AddCompany(dialog.Answer);
                SaveAllData();
                ApplySorting();
                NavigationService.NavigateTo(new CompanyElement(dialog.Answer));
            }
        }

        public void ApplySorting()
        {
            if (CompanyService == null || CheckUnappliedTop == null) return;
            var criteria = GetCurrentSortCriteria();
            var sorted = CompanyService.GetSortedList(criteria);
            Companies.Clear();
            foreach (var c in sorted) Companies.Add(c);
            if (MainFrame.Content is CompanyListPage listPage)
            {
                listPage.RefreshList();
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplySorting();
        private void SortTrigger_Click(object sender, RoutedEventArgs e) => ApplySorting();

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Einstellungen werden in Version 2.1 verfügbar sein.", "Info");
        }
        #endregion

        public CompanyService.SortCriteria GetCurrentSortCriteria()
        {
            return new CompanyService.SortCriteria
            {
                Tag = (SortComboBox?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "DateDesc",
                UnbeworbenOben = CheckUnappliedTop?.IsChecked ?? false,
                AbgelehntUnten = CheckRejectedBottom?.IsChecked ?? false,
                KeineAntwortUnten = CheckNoResponseBottom?.IsChecked ?? false
            };
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveAllData();
            base.OnClosing(e);
        }
    }
}