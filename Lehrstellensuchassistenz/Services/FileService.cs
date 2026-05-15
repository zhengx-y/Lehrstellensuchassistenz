using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Microsoft.Win32;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Resources.Languages;

namespace Lehrstellensuchassistenz.Services
{
    public class FileService
    {
        private string _basePath;
        private string _filePath;
        private string _userFilesPath;
        private string _applicationsPath;
        private readonly string _resumeName = "CV.pdf";
        private const string RegistryPath = @"Software\Lehrstellensuchassistenz";
        private const string PathValueName = "UserDataPath";
        private string _settingsPath;

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
            _applicationsPath = Path.Combine(_basePath, "applications");
            _settingsPath = Path.Combine(_basePath, "settings.json");

            try
            {
                if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
                if (!Directory.Exists(_userFilesPath)) Directory.CreateDirectory(_userFilesPath);
                if (!Directory.Exists(_applicationsPath)) Directory.CreateDirectory(_applicationsPath);
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
                // Nutzt Langs für die Fehlermeldung
                MessageBox.Show($"{Langs.MsgInfo}: {ex.Message}");
            }
        }

        public void FullCleanup(bool deleteAllUserData, bool deleteAppFolder, string currentPath)
        {
            string userDataPath = (!string.IsNullOrEmpty(currentPath) ? currentPath : GetStoragePath()).TrimEnd('\\');
            string exePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string batchFile = Path.Combine(Path.GetTempPath(), "cleanup_assistenz.bat");

            // 1. AUTOSTART ENTFERNEN
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.DeleteValue("Lehrstellensuchassistenz", false);
                }
            }
            catch { }

            // 2. REGISTRY-EINSTELLUNGEN LÖSCHEN
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

            // 3. BATCH-SKRIPT VORBEREITEN
            string commands = "@echo off\n" +
                              "timeout /t 2 > nul\n" +
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

            // 4. AUSFÜHRUNG & SHUTDOWN
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
                MessageBox.Show(ex.Message);
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
                Title = Langs.BtnChooseFolder,
                InitialDirectory = currentPath,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = Langs.BtnChooseFolder // "Ordner auswählen" Ersatz
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
                // Nutzt Langs für Titel und Filter
                var dialog = new OpenFileDialog { Title = Langs.BtnOpenResume, Filter = "PDF (*.pdf)|*.pdf" };
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        File.Copy(dialog.FileName, pdfInUserFiles, true);
                        File.Copy(pdfInUserFiles, Path.Combine(_applicationsPath, _resumeName), true);
                        OpenFile(pdfInUserFiles);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        public void OpenBewerbungenFolder()
        {
            if (Directory.Exists(_applicationsPath))
                Process.Start(new ProcessStartInfo(_applicationsPath) { UseShellExecute = true });
        }

        private void OpenFile(string path)
        {
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void SaveCustomLinks(List<SidebarLink> links)
        {
            string path = Path.Combine(GetStoragePath(), "custom_links.json");
            string json = JsonSerializer.Serialize(links, _jsonOptions);
            File.WriteAllText(path, json);
        }

        public List<SidebarLink> LoadCustomLinks()
        {
            string path = Path.Combine(GetStoragePath(), "custom_links.json");
            if (!File.Exists(path)) return new List<SidebarLink>();
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<SidebarLink>>(json, _jsonOptions) ?? new List<SidebarLink>();
            }
            catch { return new List<SidebarLink>(); }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return new AppSettings();
                string json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler beim Speichern der Settings: " + ex.Message);
            }
        }
    }
}