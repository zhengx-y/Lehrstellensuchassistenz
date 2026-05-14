using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Microsoft.Win32; // Für Registry & Dialoge
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lehrstellensuchassistenz.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var autostartService = new AutostartService();
            CbAutostart.IsChecked = autostartService.IsAutostartEnabled();

            var fileService = new FileService();
            PathTextBox.Text = fileService.GetStoragePath();
        }

        private void CbAutostart_Changed(object sender, RoutedEventArgs e)
        {
            // Verhindert Fehler beim ersten Laden
            if (!IsLoaded) return;

            var autostartService = new AutostartService();
            autostartService.SetAutostart(CbAutostart.IsChecked ?? false);
        }

        private void ChangeStoragePath_Click(object sender, RoutedEventArgs e)
        {
            var fileService = new FileService();
            string oldPath = PathTextBox.Text;

            // 1. Dialog öffnen
            string newPath = fileService.ChooseAndSaveNewPath(oldPath);

            // Nur fortfahren, wenn ein Pfad gewählt wurde und dieser anders ist als der alte
            if (string.IsNullOrEmpty(newPath) || newPath.Equals(oldPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // UI SOFORT AKTUALISIEREN
            PathTextBox.Text = newPath;

            var moveResult = MessageBox.Show(
                "Möchtest du deine vorhandenen Daten an den neuen Ort verschieben?\n\n" +
                "Ja: Daten umziehen & neuen Pfad nutzen.\n" +
                "Nein: Nur neuen Pfad nutzen (leere Datenbank).\n" +
                "Abbrechen: Nichts ändern.",
                "Daten umziehen", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (moveResult == MessageBoxResult.Cancel)
            {
                PathTextBox.Text = oldPath;
                return;
            }

            try
            {
                if (moveResult == MessageBoxResult.Yes)
                {
                    fileService.MigrateData(oldPath, newPath);
                }
                else
                {
                    fileService.SaveStoragePath(newPath);
                }

                MessageBox.Show("Speicherort aktualisiert. Die App startet nun neu.", "Neustart erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);

                // --- SICHERER NEUSTART ---
                var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (processPath != null)
                {
                    System.Diagnostics.Process.Start(processPath);
                    Application.Current.Shutdown();
                }
                else
                {
                    // Fallback falls MainModule null ist (extrem selten)
                    MessageBox.Show("Bitte starten Sie die App manuell neu.");
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Verschieben der Daten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                PathTextBox.Text = oldPath; // Rollback in der UI
            }
        }

        private void DeleteAllData_Click(object sender, RoutedEventArgs e)
        {
            bool delUserData = CbDeleteAllUserData.IsChecked ?? false;
            bool delApp = CbDeleteAppFolder.IsChecked ?? false;

            if (!delUserData && !delApp) return;

            var result = MessageBox.Show(
                "Bist du sicher? Die ausgewählten Aktionen werden sofort ausgeführt. Die App wird anschließend beendet.",
                "Cleanup Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var fileService = new FileService();
                fileService.FullCleanup(delUserData, delApp, PathTextBox.Text);

                Application.Current.Shutdown();
            }
        }

        private void PerformCleanup()
        {
            // Wir nutzen die neuen Namen aus der XAML
            bool delUserData = CbDeleteAllUserData.IsChecked ?? false;
            bool delApp = CbDeleteAppFolder.IsChecked ?? false;

            // Nur ausführen, wenn mindestens eins gewählt ist
            if (!delUserData && !delApp)
            {
                MessageBox.Show("Bitte wähle mindestens eine Option zum Löschen aus.");
                return;
            }

            var fileService = new FileService();

            // Aufruf mit den neuen Variablen
            fileService.FullCleanup(delUserData, delApp, PathTextBox.Text);

            MessageBox.Show("Die gewählten Daten wurden entfernt. Die Anwendung wird beendet.",
                            "Cleanup erfolgreich");

            Application.Current.Shutdown();
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Wir brauchen das aktuelle Window für den Service
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    var shortcutService = new ShortcutService(parentWindow);
                    shortcutService.CreateDesktopShortcut();

                    MessageBox.Show("Verknüpfung wurde erfolgreich auf dem Desktop erstellt.",
                                    "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Erstellen der Verknüpfung: " + ex.Message);
            }
        }

        private void AddCustomLink_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtLinkName.Text;
            string url = TxtLinkUrl.Text;

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(url))
            {
                var newLink = new SidebarLink { Name = name, Url = url };

                // 1. Zugriff auf das MainWindow
                var mainWindow = (MainWindow)Application.Current.MainWindow;

                // 2. In die Liste im MainWindow werfen -> Sidebar aktualisiert sich SOFORT
                mainWindow.CustomLinks.Add(newLink);

                // 3. Dauerhaft speichern (über FileService)
                var fileService = new FileService();
                var allLinks = new List<SidebarLink>(mainWindow.CustomLinks);
                fileService.SaveCustomLinks(allLinks);

                // Felder leeren
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