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
        // Wichtig: Wir nutzen Companies (Großgeschrieben) als öffentlichen Zugriffspunkt
        public ObservableCollection<Company> Companies { get; } = new ObservableCollection<Company>();
        public ObservableCollection<SidebarLink> CustomLinks { get; } = new ObservableCollection<SidebarLink>();

        private readonly FileService _fileService = new FileService();
        public readonly CompanyService CompanyService;
        public readonly NavigationService NavigationService;
        private readonly UIService _uiService = new UIService();
        private readonly MultiSelectService _multiSelectService;
        private bool _isSorting = false;

        public MainWindow()
        {
            InitializeComponent();

            CompanyService = new CompanyService(Companies);
            NavigationService = new NavigationService(MainFrame);
            _multiSelectService = new MultiSelectService(Companies);

            LoadAllData();
            LoadCustomLinks();

            NavigationService.NavigateTo(new CompanyListPage(Companies));

            this.Loaded += MainWindow_Loaded;
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var welcomeService = new WelcomeService(this);
            welcomeService.CheckFirstStart();
        }

        #region Speichern & Laden
        private void LoadAllData()
        {
            var loaded = _fileService.Load();
            Companies.Clear();
            foreach (var c in loaded) Companies.Add(c);
            ApplySorting();
        }

        public void LoadCustomLinks()
        {
            var links = _fileService.LoadCustomLinks();
            CustomLinks.Clear();
            foreach (var link in links) CustomLinks.Add(link);
        }

        public void SaveAllData()
        {
            // Hier prüfen wir, ob wir in der Detailansicht sind und synchronisieren
            if (MainFrame.Content is CompanyElement detailPage)
            {
                // Falls diese Methode in CompanyElement fehlt, siehe Schritt 2!
                detailPage.SyncNotizenToModel();
            }

            _fileService.Save(Companies);
        }

        private void SaveAllData_Click(object sender, RoutedEventArgs e)
        {
            SaveAllData();
            MessageBox.Show("Gespeichert!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            ApplySorting();
        }
        #endregion

        #region UI & Navigation
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S) { SaveAllData(); ApplySorting(); e.Handled = true; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N) { AddCompany_Click(this, new RoutedEventArgs()); e.Handled = true; }
            if (e.Key == Key.Delete && !IsUserTyping()) { BulkDelete_Click(this, new RoutedEventArgs()); e.Handled = true; }
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

        private Thickness ScaleThickness(Thickness old, double factor) => new Thickness(old.Left * factor, old.Top * factor, old.Right * factor, old.Bottom * factor);

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

        private bool IsUserTyping() => FocusManager.GetFocusedElement(this) is TextBox || FocusManager.GetFocusedElement(this) is RichTextBox;

        private void Quicklink_Click(object sender, RoutedEventArgs e) { if (sender is Button btn && btn.Tag is string url) BrowserService.OpenUrl(url); }
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
            if (CompanyService == null || CheckUnappliedTop == null || _isSorting) return;
            try
            {
                _isSorting = true;
                var criteria = GetCurrentSortCriteria();
                var sorted = CompanyService.GetSortedList(criteria);
                Companies.Clear();
                foreach (var c in sorted) Companies.Add(c);
                if (MainFrame.Content is CompanyListPage listPage) listPage.RefreshList();
            }
            finally { _isSorting = false; }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplySorting();
        private void SortTrigger_Click(object sender, RoutedEventArgs e) => ApplySorting();
        private void Settings_Click(object sender, RoutedEventArgs e) => NavigationService.NavigateTo(new SettingsPage());

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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { SaveAllData(); base.OnClosing(e); }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) { }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (var company in Companies) company.IsSelectedForAction = false;
            if (MainFrame.Content is CompanyListPage listPage) listPage.CompanyListBox.Items.Refresh();
        }

        private void BulkDelete_Click(object sender, RoutedEventArgs e)
        {
            bool hasBulk = false;
            foreach (var c in Companies) if (c.IsSelectedForAction) { hasBulk = true; break; }

            if (hasBulk)
            {
                if (_multiSelectService.DeleteSelected()) FinishBulkChange();
            }
            else
            {
                Company? toDelete = (MainFrame.Content is CompanyListPage lp) ? lp.SelectedCompany : (MainFrame.Content is CompanyElement dp ? dp.Company : null);
                if (toDelete != null && CompanyService.ConfirmAndDelete(toDelete))
                {
                    if (MainFrame.Content is CompanyElement) NavigationService.NavigateTo(new CompanyListPage(Companies));
                    FinishBulkChange();
                }
            }
        }

        private void FinishBulkChange() { SaveAllData(); ApplySorting(); }

        private void BulkStatus_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (BulkStatusCombo?.SelectedItem is ApplicationStatus status && BulkStatusCombo.IsDropDownOpen)
            {
                _multiSelectService.ChangeStatusForSelected(status);
                BulkStatusCombo.SelectedIndex = -1;
                FinishBulkChange();
            }
        }
        #endregion
    }
}