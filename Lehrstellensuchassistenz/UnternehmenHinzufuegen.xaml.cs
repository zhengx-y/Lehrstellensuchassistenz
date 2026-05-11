using System.Windows;

namespace Lehrstellensuchassistenz
{
    public partial class UnternehmenHinzufuegen : Window
    {
        public Unternehmen? Answer { get; private set; }

        public UnternehmenHinzufuegen()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Answer = new Unternehmen
            {
                Name = InputTextBox.Text,
                Website = null,
                Notizen = null,
                FotoReferenz = null,
                BewerbungsStatus = Status.Unbeworben
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
