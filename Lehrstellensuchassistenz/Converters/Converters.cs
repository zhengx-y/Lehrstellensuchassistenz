using Lehrstellensuchassistenz.Models;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lehrstellensuchassistenz.Converters
{
    // 1. Wandelt den Status in eine Farbe um
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ApplicationStatus status)
            {
                var resources = Application.Current.Resources;

                return status switch
                {
                    ApplicationStatus.Unbeworben => (SolidColorBrush?)resources["WartendBrush"] ?? Brushes.Gray,
                    ApplicationStatus.Beworben => (SolidColorBrush?)resources["BeworbenBrush"] ?? Brushes.DarkBlue,
                    ApplicationStatus.Abgelehnt => (SolidColorBrush?)resources["AbgelehntBrush"] ?? Brushes.DarkRed,
                    ApplicationStatus.Zusage => (SolidColorBrush?)resources["AngenommenBrush"] ?? Brushes.Green,
                    ApplicationStatus.Eingeladen => Brushes.DarkOrange,
                    ApplicationStatus.KeineAntwort => Brushes.LightGray,
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. Zeigt Elemente nur an, wenn der Wert nicht NULL ist
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 3. Wahrheitswert zu Sichtbarkeit
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 4. Holt den Text aus dem [Description] Attribut des Enums
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) return value.ToString();

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}