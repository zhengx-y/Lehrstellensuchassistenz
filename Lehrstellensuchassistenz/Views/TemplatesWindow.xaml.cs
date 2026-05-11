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
                // Zeigt immer auf den Ordner, in dem die .exe aktuell gestartet wurde
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz");

                switch (type)
                {
                    case ApplicationType.Generic:
                        sourceFile = Path.Combine(baseDir, "Resources", "Templates", "Application_Example.docx");
                        break;

                    case ApplicationType.Empty:
                        sourceFile = Path.Combine(baseDir, "Resources", "Templates", "Empty.docx");
                        break;

                    case ApplicationType.Custom:
                        sourceFile = Path.Combine(appDataPath, "user-files", "Eigen_Vorlage.docx");
                        break;
                }

                // Pfad für Windows "sauber" machen (entfernt doppelte Backslashes etc.)
                if (sourceFile != null) sourceFile = Path.GetFullPath(sourceFile);

                // Check, ob die Datei jetzt wirklich im bin-Ordner existiert
                if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
                {
                    MessageBox.Show($"Datei nicht gefunden!\nPfad: {sourceFile}\n\nBitte prüfen Sie, ob der Ordner 'Resources' neben der Programm-Datei liegt.",
                                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // --- Ziel-Logik (Bewerbung erstellen) ---
                string targetFolder = Path.Combine(appDataPath, "bewerbungen");
                Directory.CreateDirectory(targetFolder);

                string companyName = _company?.Name ?? "Bewerbung";
                foreach (char c in Path.GetInvalidFileNameChars()) companyName = companyName.Replace(c, '_');

                string newFileName = $"{companyName}_{DateTime.Now:yyyyMMdd_HHmm}.docx";
                string targetPath = Path.Combine(targetFolder, newFileName);

                File.Copy(sourceFile, targetPath, true);

                // Model & UI Update
                if (_company != null)
                {
                    _company.LastApplicationPath = targetPath;
                    _company.Status = ApplicationStatus.Beworben;
                    _company.UpdateTimestamp();
                }

                _companyElement?.LoadNotizen();

                // Dokument öffnen
                Process.Start(new ProcessStartInfo(targetPath) { UseShellExecute = true });

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }
    }
}