using System;
using System.Windows;
using Lehrstellensuchassistenz.Models;
using Lehrstellensuchassistenz.Resources.Languages; // Wichtig für Langs

namespace Lehrstellensuchassistenz.Views
{
    public partial class AddCompany : Window
    {
        // Das Ergebnis, das das MainWindow nach dem Schließen abruft
        public Company? Answer { get; private set; }

        public AddCompany()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Validierung: Name darf nicht leer sein
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                // Nutzt Langs für Fehlermeldung und Titel
                MessageBox.Show(Langs.LblQuestionCompanyName, Langs.MsgInfo, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Neues Objekt erstellen
            Answer = new Company
            {
                Name = InputTextBox.Text.Trim(),
                Website = "",
                Status = ApplicationStatus.Unbeworben,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now
            };

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }
    }
}