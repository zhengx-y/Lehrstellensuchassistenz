using Lehrstellensuchassistenz.Resources.Languages; // Wichtig für den Zugriff auf Resources
using Lehrstellensuchassistenz.Services;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

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
                _parentWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ShowWelcomeInfo();
                    ShowInitialSetup();

                    using var keyUpdate = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                    keyUpdate?.SetValue(WelcomeShownValue, "Yes");
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        private void ShowWelcomeInfo()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(25) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = Lehrstellensuchassistenz.Resources.Languages.Langs.WelcomeInfoTitle, // Ressource statt Hardcoded
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = System.Windows.Media.Brushes.DarkBlue
            });

            // Der InfoText wird jetzt aus der Ressource geladen
            // Tipp: In der .resx kannst du Shift+Enter für Zeilenumbrüche nutzen!
            stackPanel.Children.Add(new TextBlock
            {
                Text = Lehrstellensuchassistenz.Resources.Languages.Langs.WelcomeInfoContent,
                FontSize = 16,
                LineHeight = 24,
                TextWrapping = TextWrapping.Wrap
            });

            var btnClose = new Button
            {
                Content = Lehrstellensuchassistenz.Resources.Languages.Langs.BtnUnderstood, // Ressource
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
                Title = Lehrstellensuchassistenz.Resources.Languages.Langs.TitleWelcome, // Ressource
                SizeToContent = SizeToContent.WidthAndHeight,
                Width = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _parentWindow,
                ResizeMode = ResizeMode.NoResize,
                Content = stackPanel,
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            btnClose.Click += (s, e) => infoWindow.Close();
            infoWindow.ShowDialog();
        }

        private void ShowInitialSetup()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var cbShortcut = new CheckBox { Content = Lehrstellensuchassistenz.Resources.Languages.Langs.CheckCreateShortcut, IsChecked = true, Margin = new Thickness(0, 10, 0, 5) };
            var cbAutostart = new CheckBox { Content = Lehrstellensuchassistenz.Resources.Languages.Langs.CheckAutostart, IsChecked = false, Margin = new Thickness(0, 5, 0, 20) };
            var btnSave = new Button { Content = Lehrstellensuchassistenz.Resources.Languages.Langs.BtnFinishSetup, Width = 150, Height = 30 };

            stackPanel.Children.Add(new TextBlock { Text = Lehrstellensuchassistenz.Resources.Languages.Langs.HeaderQuickSetup, FontWeight = FontWeights.Bold, FontSize = 16 });
            stackPanel.Children.Add(cbShortcut);
            stackPanel.Children.Add(cbAutostart);
            stackPanel.Children.Add(btnSave);

            var setupWindow = new Window
            {
                Title = Lehrstellensuchassistenz.Resources.Languages.Langs.TitleSettings, // Ressource
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _parentWindow,
                ResizeMode = ResizeMode.NoResize,
                Content = stackPanel
            };

            btnSave.Click += (s, e) =>
            {
                if (cbShortcut.IsChecked == true) _shortcutService.CreateDesktopShortcut();
                _autostartService.SetAutostart(cbAutostart.IsChecked == true);
                setupWindow.Close();
            };

            setupWindow.ShowDialog();
        }
    }
}