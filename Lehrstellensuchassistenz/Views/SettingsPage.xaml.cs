using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Lehrstellensuchassistenz.Resources.Languages; // Wichtig
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Lehrstellensuchassistenz.Views
{
    public partial class SettingsPage : Page
    {
        private bool _isPageInitialized = false;

        public SettingsPage()
        {
            InitializeComponent();
            LoadCurrentSettings();
            LoadSettingsIntoUI();
            _isPageInitialized = true;
        }

        private void LoadCurrentSettings()
        {
            var autostartService = new AutostartService();
            CbAutostart.IsChecked = autostartService.IsAutostartEnabled();

            var fileService = new FileService();
            PathTextBox.Text = fileService.GetStoragePath();
        }

        private void LoadSettingsIntoUI()
        {
            var settings = new FileService().LoadSettings();
            foreach (ComboBoxItem item in ComboLanguage.Items)
            {
                if (item.Tag?.ToString() == settings.Language)
                {
                    ComboLanguage.SelectedItem = item;
                    break;
                }
            }
        }

        private void ComboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isPageInitialized) return;

            if (ComboLanguage.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureCode = selectedItem.Tag.ToString();
                var service = new FileService();
                var settings = service.LoadSettings();

                if (settings.Language != cultureCode)
                {
                    settings.Language = cultureCode;
                    service.SaveSettings(settings);

                    // Benutzt HintRestartNote und MsgInfo
                    MessageBox.Show(Langs.HintRestartNote, Langs.MsgInfo, MessageBoxButton.OK, MessageBoxImage.Information);

                    RestartApplication();
                }
            }
        }

        private void CbAutostart_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isPageInitialized) return;
            var autostartService = new AutostartService();
            autostartService.SetAutostart(CbAutostart.IsChecked ?? false);
        }

        private void ChangeStoragePath_Click(object sender, RoutedEventArgs e)
        {
            var fileService = new FileService();
            string oldPath = PathTextBox.Text;
            string newPath = fileService.ChooseAndSaveNewPath(oldPath);

            if (string.IsNullOrEmpty(newPath) || newPath.Equals(oldPath, StringComparison.OrdinalIgnoreCase))
                return;

            PathTextBox.Text = newPath;

            // Neuer Key: MsgMigrateQuestion
            var moveResult = MessageBox.Show(Langs.MsgMigrateQuestion, Langs.MsgConfirmDeleteTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (moveResult == MessageBoxResult.Cancel)
            {
                PathTextBox.Text = oldPath;
                return;
            }

            try
            {
                if (moveResult == MessageBoxResult.Yes)
                    fileService.MigrateData(oldPath, newPath);
                else
                    fileService.SaveStoragePath(newPath);

                // Neuer Key: MsgPathUpdatedRestart
                MessageBox.Show(Langs.MsgPathUpdatedRestart, Langs.MsgInfo, MessageBoxButton.OK, MessageBoxImage.Information);

                RestartApplication();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Langs.MsgInfo}: {ex.Message}", Langs.MsgInfo, MessageBoxButton.OK, MessageBoxImage.Error);
                PathTextBox.Text = oldPath;
            }
        }

        private void RestartApplication()
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath)) exePath = Environment.ProcessPath;

                if (!string.IsNullOrEmpty(exePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                    Environment.Exit(0);
                }
                else
                {
                    // Hier könnte man noch einen Key "ErrExeNotFound" hinzufügen falls nötig
                    Application.Current.Shutdown();
                }
            }
            catch { Environment.Exit(0); }
        }

        private void DeleteAllData_Click(object sender, RoutedEventArgs e)
        {
            bool delUserData = CbDeleteAllUserData.IsChecked ?? false;
            bool delApp = CbDeleteAppFolder.IsChecked ?? false;

            if (!delUserData && !delApp) return;

            // Neuer Key: MsgCleanupConfirm
            var result = MessageBox.Show(Langs.MsgCleanupConfirm, Langs.MsgConfirmDeleteTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                new FileService().FullCleanup(delUserData, delApp, PathTextBox.Text);
                Application.Current.Shutdown();
            }
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                new ShortcutService(parentWindow).CreateDesktopShortcut();
                MessageBox.Show(Langs.MsgSaved);
            }
        }

        private void AddCustomLink_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtLinkName.Text;
            string url = TxtLinkUrl.Text;

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(url))
            {
                var newLink = new SidebarLink { Name = name, Url = url };
                var mainWindow = (MainWindow)Application.Current.MainWindow;

                mainWindow.CustomLinks.Add(newLink);
                new FileService().SaveCustomLinks(new List<SidebarLink>(mainWindow.CustomLinks));

                TxtLinkName.Clear();
                TxtLinkUrl.Clear();
            }
        }

        private void ManageLinks_Click(object sender, RoutedEventArgs e)
        {
            var deleteWin = new DeleteCustomLinksWindow { Owner = Application.Current.MainWindow };
            deleteWin.ShowDialog();
        }
    }
}