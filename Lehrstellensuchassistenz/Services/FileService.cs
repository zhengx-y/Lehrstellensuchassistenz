using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Microsoft.Win32;
using Lehrstellensuchassistenz.Models; // WICHTIG: Namespace für Company

namespace Lehrstellensuchassistenz.Services
{
    public class FileService
    {
        private readonly string _basePath;
        private readonly string _filePath;
        private readonly string _userFilesPath;
        private readonly string _bewerbungenPath;
        private readonly string _resumeName = "Lebenslauf.pdf";

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public FileService()
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz");
            _filePath = Path.Combine(_basePath, "companies.json");
            _userFilesPath = Path.Combine(_basePath, "user-files");
            _bewerbungenPath = Path.Combine(_basePath, "bewerbungen");

            // Sicherstellen, dass alle Ordner existieren
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(_userFilesPath);
            Directory.CreateDirectory(_bewerbungenPath);
        }

        // Geändert: List<Unternehmen> -> List<Company>
        public List<Company> Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return new List<Company>();

                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<Company>>(json, _jsonOptions) ?? new List<Company>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler beim Laden: " + ex.Message);
                return new List<Company>();
            }
        }

        // Geändert: IEnumerable<Unternehmen> -> IEnumerable<Company>
        public void Save(IEnumerable<Company> companies)
        {
            try
            {
                // Nur zur Sicherheit, falls jemand den Ordner im laufenden Betrieb löscht
                Directory.CreateDirectory(_basePath);

                string json = JsonSerializer.Serialize(companies, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}");
            }
        }

        public void OpenOrSelectResume()
        {
            string pdfInUserFiles = Path.Combine(_userFilesPath, _resumeName);

            if (File.Exists(pdfInUserFiles))
            {
                OpenFile(pdfInUserFiles);
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Wähle deinen Lebenslauf aus",
                    Filter = "PDF Dokumente (*.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        File.Copy(dialog.FileName, pdfInUserFiles, true);

                        string pdfInBewerbungen = Path.Combine(_bewerbungenPath, _resumeName);
                        File.Copy(pdfInUserFiles, pdfInBewerbungen, true);

                        OpenFile(pdfInUserFiles);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fehler beim Kopieren der Datei: " + ex.Message);
                    }
                }
            }
        }

        public void OpenBewerbungenFolder()
        {
            if (Directory.Exists(_bewerbungenPath))
            {
                Process.Start(new ProcessStartInfo(_bewerbungenPath) { UseShellExecute = true });
            }
        }

        private void OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Konnte Datei nicht öffnen: " + ex.Message);
            }
        }
    }
}