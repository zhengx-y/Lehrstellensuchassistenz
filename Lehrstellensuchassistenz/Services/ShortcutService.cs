using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Lehrstellensuchassistenz.Services
{
    public class ShortcutService
    {
        private const string RegistryKeyPath = @"SOFTWARE\Lehrstellensuchassistenz";
        private const string RegistryValueName = "DesktopShortcutAsked";
        private const string WelcomeShownValue = "WelcomeShown";

        // Wir brauchen eine Referenz auf das Hauptfenster für Popups
        private readonly Window _parentWindow;

        public ShortcutService(Window parentWindow)
        {
            _parentWindow = parentWindow;
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
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    CreateDesktopShortcut();
                    key?.SetValue(RegistryValueName, "Yes");
                }
                else if (result == MessageBoxResult.No)
                {
                    key?.SetValue(RegistryValueName, "No");
                }
            }
        }

        private void CreateDesktopShortcut()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutLocation = Path.Combine(desktopPath, "Lehrstellensuchassistenz.lnk");
                string exePath = Process.GetCurrentProcess().MainModule!.FileName;

                // Icon Pfad korrigiert auf die neue Struktur
                string projectFolder = Path.GetDirectoryName(exePath)!;
                string iconPath = Path.Combine(projectFolder, "resources", "images", "shortcut_icon.ico");

                if (!System.IO.File.Exists(iconPath))
                {
                    iconPath = exePath; // Fallback
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

        public void ShowWelcomePopup()
        {
            // Da dies oft beim Start aufgerufen wird, nutzen wir den Dispatcher des Hauptfensters
            _parentWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                object? value = key?.GetValue(WelcomeShownValue);

                if (value == null)
                {
                    var textBlock = new TextBlock
                    {
                        Text = @"Willkommen bei der Lehrstellensuchassistenz!
Oben findest du die wichtigsten Buttons:
- 'Zurück': Navigiert zur vorherigen Seite
- 'Änderungen speichern': Speichert deine Liste
- 'Löschen': Entfernt die gewählte Firma

Sidebar & Suche:
- 'Lebenslauf': PDF hochladen oder öffnen
- Portale: Direkt-Links zu AMS, karriere.at etc.

Tipp: Die Ansicht kann mit Strg + Mausrad gezoomt werden.",
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20)
                    };

                    var closeButton = new Button
                    {
                        Content = "Verstanden",
                        Width = 120,
                        Height = 35,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    };

                    var popup = new Window
                    {
                        Title = "Willkommen!",
                        SizeToContent = SizeToContent.Height,
                        Width = 500,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = _parentWindow, // Hier nutzen wir den übergebenen Owner
                        ResizeMode = ResizeMode.NoResize,
                        Content = new StackPanel { Children = { textBlock, closeButton } }
                    };

                    closeButton.Click += (s, e) => popup.Close();
                    popup.ShowDialog();

                    key?.SetValue(WelcomeShownValue, "Yes");
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}