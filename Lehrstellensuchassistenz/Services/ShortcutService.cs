using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Lehrstellensuchassistenz.Services
{
    public class ShortcutService
    {
        private const string RegistryKeyPath = @"SOFTWARE\Lehrstellensuchassistenz";
        private const string RegistryValueName = "DesktopShortcutAsked";

        private readonly Window _parentWindow;

        public ShortcutService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        // Jetzt PUBLIC, damit WelcomeService darauf zugreifen kann
        public void CreateDesktopShortcut()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutLocation = Path.Combine(desktopPath, "Lehrstellensuchassistenz.lnk");
                string exePath = Process.GetCurrentProcess().MainModule!.FileName;

                string projectFolder = Path.GetDirectoryName(exePath)!;
                string iconPath = Path.Combine(projectFolder, "resources", "images", "shortcut_icon.ico");

                if (!System.IO.File.Exists(iconPath))
                {
                    iconPath = exePath;
                }

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
                shortcut.Description = "Lehrstellensuchassistenz";
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.IconLocation = iconPath;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Erstellen der Verknüpfung: " + ex.Message);
            }
        }

        public void CheckDesktopShortcut()
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            object? value = key?.GetValue(RegistryValueName);

            if (value == null)
            {
                var result = MessageBox.Show(
                    "Soll eine Verknüpfung auf dem Desktop erstellt werden?",
                    "Verknüpfung erstellen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    CreateDesktopShortcut();
                    key?.SetValue(RegistryValueName, "Yes");
                }
                else
                {
                    key?.SetValue(RegistryValueName, "No");
                }
            }
        }
    }
}