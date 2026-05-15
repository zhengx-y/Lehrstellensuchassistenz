using Lehrstellensuchassistenz.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lehrstellensuchassistenz.Converters
{
    // 1. NEU: Checkt ob Firmen in einer Liste markiert sind (für die Top-Bar Sichtbarkeit)
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Wir prüfen, ob in der Collection irgendeine Firma IsSelectedForAction = true hat
            if (value is IEnumerable<Company> companies)
            {
                return companies.Any(c => c.IsSelectedForAction) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 2. Farben-Converter (Optimiert auf Performance)
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ApplicationStatus status)
            {
                var resources = Application.Current.Resources;

                return status switch
                {
                    ApplicationStatus.Unbeworben => resources["WartendBrush"] as SolidColorBrush ?? Brushes.Gray,
                    ApplicationStatus.Beworben => resources["BeworbenBrush"] as SolidColorBrush ?? Brushes.DarkBlue,
                    ApplicationStatus.Abgelehnt => resources["AbgelehntBrush"] as SolidColorBrush ?? Brushes.DarkRed,
                    ApplicationStatus.Zusage => resources["AngenommenBrush"] as SolidColorBrush ?? Brushes.Green,
                    ApplicationStatus.Eingeladen => Brushes.DarkOrange,
                    ApplicationStatus.KeineAntwort => Brushes.LightGray,
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 3. Null/Empty Check (Verbessert für Strings)
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is string s && string.IsNullOrWhiteSpace(s)) return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 4. Bool Check (Mit Invert-Option)
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (value is bool b) && b;
            if (parameter as string == "Invert") val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v) return v == Visibility.Visible;
            return false;
        }
    }

    // 5. Text-Übersetzung (Gefixed für Two-Way Binding)
    public class EnumToLocalizedNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ApplicationStatus status)
            {
                return status.ToDisplayName();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Fix: Wenn die ComboBox den Text zurückgibt, muss dieser wieder in das Enum gewandelt werden.
            // Da WPF ComboBoxen oft das Enum-Member selbst als DataContext halten, 
            // ist Binding.DoNothing oft sicherer, wenn das Binding im Model nur 'OneWay' ist.
            return Binding.DoNothing;
        }
    }
}