using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using Lehrstellensuchassistenz.Services;

namespace Lehrstellensuchassistenz.Services
{
    public class WelcomeService
    {
        private readonly Window _parentWindow;
        private readonly ShortcutService _shortcutService;
        private readonly AutostartService _autostartService;
        private const string RegistryKeyPath = @"SOFTWARE\Lehrstellensuchassistenz";
        private const string WelcomeShownValue = "WelcomeShown";

        public WelcomeService(Window parentWindow)
        {
            _parentWindow = parentWindow;
            _shortcutService = new ShortcutService(parentWindow);
            _autostartService = new AutostartService();
        }

        public void CheckFirstStart()
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key?.GetValue(WelcomeShownValue) == null)
            {
                // Wir schicken die Ausführung ans Ende der Prioritäten-Schlange.
                // Dadurch wird erst das MainWindow komplett gezeichnet.
                _parentWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ShowWelcomeInfo();
                    ShowInitialSetup();

                    // Erst nach Abschluss der Dialoge in Registry schreiben
                    using var keyUpdate = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                    keyUpdate?.SetValue(WelcomeShownValue, "Yes");
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        private void ShowWelcomeInfo()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(25) };

            // Titel - Jetzt etwas dezenter
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Erste Schritte & Hilfe",
                FontSize = 18, // Kleinerer Titel
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = System.Windows.Media.Brushes.DarkBlue
            });

            // Der Text mit Fokus auf den Bewerbungsordner
            string infoText =
                "Willkommen bei der Lehrstellensuchassistenz!\n\n" +
                "📌 DEINE DATEIEN & ORDNER:\n" +
                "• Bewerbungsordner: Hier findest du alle deine Dateien (Lebenslauf, Zeugnisse).\n" +
                "• Lebenslauf öffnen: Startet direkt deine hinterlegte PDF-Datei.\n\n" +
                "🚀 APP-BEDIENUNG:\n" +
                "• + Firma hinzufügen: Erstellt einen neuen Eintrag.\n" +
                "• Änderungen speichern: Sichert deine Notizen (Hotkey: STRG + S).\n" +
                "• Portale: Schneller Zugriff auf AMS, karriere.at & Co.\n\n" +
                "⌨ TASTATUR-TIPPS:\n" +
                "• ENTF: Markierte Firma löschen.\n" +
                "• ESC: Aktuelle Ansicht schließen / Zurück.\n" +
                "• STRG + Mausrad: Zoomt die gesamte App stufenlos.\n";

            stackPanel.Children.Add(new TextBlock
            {
                Text = infoText,
                FontSize = 16, // Schön lesbare Inhaltsgröße
                LineHeight = 24,
                TextWrapping = TextWrapping.Wrap
            });

            var btnClose = new Button
            {
                Content = "Verstanden",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = System.Windows.Media.Brushes.WhiteSmoke,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            stackPanel.Children.Add(btnClose);

            var infoWindow = new Window
            {
                Title = "Willkommen",
                SizeToContent = SizeToContent.WidthAndHeight,
                Width = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _parentWindow,
                ResizeMode = ResizeMode.NoResize,
                Content = stackPanel,
                // Optional: Ein schöner Rahmen
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            btnClose.Click += (s, e) => infoWindow.Close();
            infoWindow.ShowDialog();
        }

        private void ShowInitialSetup()
        {
            // Wir bauen ein kleines Fenster mit zwei Checkboxen
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var cbShortcut = new CheckBox { Content = "Desktop-Verknüpfung erstellen", IsChecked = true, Margin = new Thickness(0, 10, 0, 5) };
            var cbAutostart = new CheckBox { Content = "App beim Windows-Start automatisch öffnen", IsChecked = false, Margin = new Thickness(0, 5, 0, 20) };
            var btnSave = new Button { Content = "Einrichtung abschließen", Width = 150, Height = 30 };

            stackPanel.Children.Add(new TextBlock { Text = "Schnell-Einrichtung", FontWeight = FontWeights.Bold, FontSize = 16 });
            stackPanel.Children.Add(cbShortcut);
            stackPanel.Children.Add(cbAutostart);
            stackPanel.Children.Add(btnSave);

            var setupWindow = new Window
            {
                Title = "Einstellungen",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _parentWindow,
                ResizeMode = ResizeMode.NoResize,
                Content = stackPanel
            };

            btnSave.Click += (s, e) =>
            {
                // Verknüpfung erstellen wenn gewählt
                if (cbShortcut.IsChecked == true) _shortcutService.CreateDesktopShortcut();

                // Autostart setzen
                _autostartService.SetAutostart(cbAutostart.IsChecked == true);

                setupWindow.Close();
            };

            setupWindow.ShowDialog();
        }
    }
}