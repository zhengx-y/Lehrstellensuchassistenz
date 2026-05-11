using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;

namespace Lehrstellensuchassistenz.Views
{
    public partial class TemplatesWindow : Window
    {
        private readonly Company _company;
        private readonly CompanyElement _companyElement;

        // Auf Englisch umgestellt für Uniformität
        private enum ApplicationType
        {
            Generic,
            Empty,
            Custom
        }

        public TemplatesWindow(Company company, CompanyElement companyElement)
        {
            InitializeComponent();
            _company = company;
            _companyElement = companyElement;
        }

        // --- Event Handler (Passend zur neuen XAML) ---

        private void SelectGenericTemplate_Click(object sender, RoutedEventArgs e)
            => CreateApplication(ApplicationType.Generic);

        private void SelectEmptyTemplate_Click(object sender, RoutedEventArgs e)
            => CreateApplication(ApplicationType.Empty);

        private void UseCustomTemplate_Click(object sender, RoutedEventArgs e)
            => CreateApplication(ApplicationType.Custom);

        private void UploadCustomTemplate_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Eigene Vorlage hochladen",
                Filter = "Word-Dokumente (*.docx;*.doc)|*.docx;*.doc"
            };

            if (dialog.ShowDialog() == true)
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz");
                string userFilesPath = Path.Combine(appDataPath, "user-files");

                Directory.CreateDirectory(userFilesPath);
                string targetPath = Path.Combine(userFilesPath, "Eigen_Vorlage.docx");

                try
                {
                    File.Copy(dialog.FileName, targetPath, true);
                    MessageBox.Show("Vorlage erfolgreich gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Speichern: " + ex.Message);
                }
            }
        }

        // --- Hauptlogik ---

        private void CreateApplication(ApplicationType type)
        {
            try
            {
                string? sourceFile = null;
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz");

                switch (type)
                {
                    case ApplicationType.Generic:
                        sourceFile = Path.Combine(basePath, "Resources", "Templates", "Application_Example.docx");
                        break;

                    case ApplicationType.Empty:
                        sourceFile = Path.Combine(basePath, "Resources", "Templates", "Empty.docx");
                        break;

                    case ApplicationType.Custom:
                        sourceFile = Path.Combine(appDataPath, "user-files", "Eigen_Vorlage.docx");
                        if (!File.Exists(sourceFile))
                        {
                            MessageBox.Show("Bitte laden Sie erst eine eigene Vorlage hoch.", "Vorlage fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        break;
                }

                if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
                {
                    MessageBox.Show("Vorlage nicht gefunden. Bitte prüfen Sie den 'Resources/Templates' Ordner.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ziel-Ordner erstellen
                string targetFolder = Path.Combine(appDataPath, "bewerbungen");
                Directory.CreateDirectory(targetFolder);

                // Dateiname säubern (keine ungültigen Zeichen)
                string companyName = _company?.Name ?? "Bewerbung";
                foreach (char c in Path.GetInvalidFileNameChars()) companyName = companyName.Replace(c, '_');

                string newFileName = $"{companyName}_{DateTime.Now:yyyyMMdd_HHmm}.docx";
                string targetPath = Path.Combine(targetFolder, newFileName);

                // Datei kopieren
                File.Copy(sourceFile, targetPath, true);

                // Model aktualisieren
                if (_company != null)
                {
                    _company.LastApplicationPath = targetPath;
                    _company.Status = ApplicationStatus.Beworben;
                    _company.UpdateTimestamp();
                }

                // UI Refresh
                _companyElement?.LoadNotizen();

                // Word Dokument öffnen
                Process.Start(new ProcessStartInfo(targetPath) { UseShellExecute = true });

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Erstellen der Bewerbung: " + ex.Message);
            }
        }
    }
}