using Lehrstellensuchassistenz;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace Lehrstellensuchassistenz
{
    public class RichTextBoxEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDocument doc)
            {
                string text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
                return string.IsNullOrWhiteSpace(text) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}