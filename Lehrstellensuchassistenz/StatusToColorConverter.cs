using Lehrstellensuchassistenz;
using System;
using System.Globalization;
using System.Windows; // WICHTIG für Application
using System.Windows.Data;
using System.Windows.Media;

namespace Lehrstellensuchassistenz
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Status status)
            {
                // Wir nutzen System.Windows.Application, um Mehrdeutigkeiten zu vermeiden
                var resources = System.Windows.Application.Current.Resources;

                return status switch
                {
                    Status.Unbeworben => (SolidColorBrush?)resources["WartendBrush"] ?? Brushes.Gray,
                    Status.Beworben => (SolidColorBrush?)resources["BeworbenBrush"] ?? Brushes.DarkBlue,
                    Status.Abgelehnt => (SolidColorBrush?)resources["AbgelehntBrush"] ?? Brushes.DarkRed,
                    Status.Angenommen => (SolidColorBrush?)resources["AngenommenBrush"] ?? Brushes.Green,

                    // Für diese hast du noch keine Keys im XAML, daher Fallback auf Standardfarben
                    Status.KeineAntwort => Brushes.LightGray,
                    Status.NächsteSchritte => Brushes.DarkOrange,
                    Status.Praktikum => Brushes.DarkGreen,

                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}