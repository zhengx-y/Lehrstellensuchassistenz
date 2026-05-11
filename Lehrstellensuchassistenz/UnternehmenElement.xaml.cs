using System.Windows.Controls;

namespace Lehrstellensuchassistenz
{
    public partial class UnternehmenElement : Page
    {
        public Unternehmen Company { get; }

        public UnternehmenElement(Unternehmen company)
        {
            InitializeComponent();
            Company = company;

            // Optional: Zeige Name im UI oder setze DataContext für Binding
            this.DataContext = Company;
        }
    }
}
