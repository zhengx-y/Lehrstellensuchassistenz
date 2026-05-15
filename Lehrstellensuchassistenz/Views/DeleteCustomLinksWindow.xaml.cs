using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Services;
using Lehrstellensuchassistenz.Resources.Languages; // Wichtig für Langs

namespace Lehrstellensuchassistenz.Views
{
    public partial class DeleteCustomLinksWindow : Window
    {
        private MainWindow _main;

        public DeleteCustomLinksWindow()
        {
            InitializeComponent();
            _main = (MainWindow)Application.Current.MainWindow;

            // Die Liste aus dem MainWindow anzeigen
            LinksListBox.ItemsSource = _main.CustomLinks;
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = new List<SidebarLink>();

            foreach (var item in LinksListBox.Items)
            {
                var container = LinksListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    var checkBox = FindVisualChild<CheckBox>(container);
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        toDelete.Add(item as SidebarLink);
                    }
                }
            }

            if (toDelete.Count > 0)
            {
                // Nutzt jetzt den extra Link-Delete-Key
                var result = MessageBox.Show(
                    $"{toDelete.Count} {Langs.MsgConfirmDeleteLinks}",
                    Langs.MsgConfirmDeleteTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var link in toDelete)
                    {
                        _main.CustomLinks.Remove(link);
                    }

                    // Speichern über FileService
                    var fs = new FileService();
                    fs.SaveCustomLinks(new List<SidebarLink>(_main.CustomLinks));
                }
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}