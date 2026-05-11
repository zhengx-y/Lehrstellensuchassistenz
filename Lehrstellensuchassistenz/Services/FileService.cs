using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Microsoft.Win32;
using Lehrstellensuchassistenz.Models;

namespace Lehrstellensuchassistenz.Services
{
    public class FileService
    {
        private string _basePath;
        private string _filePath;
        private string _userFilesPath;
        private string _bewerbungenPath;
        private readonly string _resumeName = "Lebenslauf.pdf";
        private const string RegistryPath = @"Software\Lehrstellensuchassistenz";
        private const string PathValueName = "UserDataPath";

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public FileService()
        {
            _basePath = GetStoragePath();
            _filePath = Path.Combine(_basePath, "companies.json");
            _userFilesPath = Path.Combine(_basePath, "user-files");
            _bewerbungenPath = Path.Combine(_basePath, "bewerbungen");

            try
            {
                if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
                if (!Directory.Exists(_userFilesPath)) Directory.CreateDirectory(_userFilesPath);
                if (!Directory.Exists(_bewerbungenPath)) Directory.CreateDirectory(_bewerbungenPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler beim Erstellen der Ordner: " + ex.Message);
            }
        }

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

        public void Save(IEnumerable<Company> companies)
        {
            try
            {
                Directory.CreateDirectory(_basePath);
                string json = JsonSerializer.Serialize(companies, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}");
            }
        }

        public void FullCleanup(bool deleteAllUserData, bool deleteAppFolder, string currentPath)
        {
            string userDataPath = (!string.IsNullOrEmpty(currentPath) ? currentPath : GetStoragePath()).TrimEnd('\\');
            string exePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string batchFile = Path.Combine(Path.GetTempPath(), "cleanup_assistenz.bat");

            // 1. AUTOSTART IMMER ENTFERNEN (wenn gelˆscht wird)
            // Wenn die App oder die Daten gehen, hat der Autostart keinen Sinn mehr.
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.DeleteValue("Lehrstellensuchassistenz", false);
                }
            }
            catch { /* Falls der Eintrag nie existierte, ignorieren wir den Fehler */ }

            // 2. REGISTRY-EINSTELLUNGEN DER APP L÷SCHEN
            if (deleteAllUserData)
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey("Software", true))
                    {
                        key?.DeleteSubKeyTree("Lehrstellensuchassistenz", false);
                    }
                }
                catch { }
            }

            // 3. BATCH-SKRIPT F‹R DATEIL÷SCHUNG VORBEREITEN
            string commands = "@echo off\n" +
                              "timeout /t 2 > nul\n" + // Warte 2 Sek, damit die App Zeit zum Schlieﬂen hat
                              "cd /d %temp%\n";

            if (deleteAllUserData)
            {
                commands += $"rd /s /q \"{userDataPath}\"\n";
            }

            if (deleteAppFolder)
            {
                commands += $"rd /s /q \"{exePath}\"\n";
            }

            commands += $"del \"{batchFile}\"";

            // 4. AUSF‹HRUNG & SHUTDOWN
            try
            {
                File.WriteAllText(batchFile, commands);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batchFile}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Cleanup-Skript: " + ex.Message);
            }
        }

        public string GetStoragePath()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                string savedPath = key?.GetValue(PathValueName) as string;
                if (!string.IsNullOrEmpty(savedPath)) return savedPath;
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lehrstellensuchassistenz");
        }

        public void SaveStoragePath(string newPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue(PathValueName, newPath);
            }
        }

        public string ChooseAndSaveNewPath(string currentPath)
        {
            var dialog = new OpenFileDialog
            {
                Title = "W‰hle den neuen Speicherort",
                InitialDirectory = currentPath,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Ordner ausw‰hlen"
            };

            return (dialog.ShowDialog() == true) ? Path.GetDirectoryName(dialog.FileName) : currentPath;
        }

        public void MigrateData(string oldPath, string newPath)
        {
            if (oldPath == newPath) return;
            MoveDirectoryContents(oldPath, newPath);
            SaveStoragePath(newPath);
        }

        private void MoveDirectoryContents(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                MoveDirectoryContents(directory, targetSubDir);
                Directory.Delete(directory, true);
            }
        }

        public void OpenOrSelectResume()
        {
            string pdfInUserFiles = Path.Combine(_userFilesPath, _resumeName);
            if (File.Exists(pdfInUserFiles)) { OpenFile(pdfInUserFiles); }
            else
            {
                var dialog = new OpenFileDialog { Title = "W‰hle deinen Lebenslauf", Filter = "PDF (*.pdf)|*.pdf" };
                if (dialog.ShowDialog() == true)
                {
                    try { 
                        File.Copy(dialog.FileName, pdfInUserFiles, true);
                        File.Copy(pdfInUserFiles, Path.Combine(_bewerbungenPath, _resumeName), true);
                        OpenFile(pdfInUserFiles);
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        public void OpenBewerbungenFolder()
        {
            if (Directory.Exists(_bewerbungenPath))
                Process.Start(new ProcessStartInfo(_bewerbungenPath) { UseShellExecute = true });
        }

        private void OpenFile(string path)
        {
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        public void SaveCustomLinks(System.Collections.Generic.List<SidebarLink> links)
        {
            string path = Path.Combine(GetStoragePath(), "custom_links.json");
            string json = System.Text.Json.JsonSerializer.Serialize(links);
            File.WriteAllText(path, json);
        }

        public System.Collections.Generic.List<SidebarLink> LoadCustomLinks()
        {
            string path = Path.Combine(GetStoragePath(), "custom_links.json");
            if (!File.Exists(path)) return new System.Collections.Generic.List<SidebarLink>();
            return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<SidebarLink>>(File.ReadAllText(path));
        }
    }
}