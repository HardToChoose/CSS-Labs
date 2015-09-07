using System;
using System.Diagnostics.Contracts;
using System.Windows.Data;
using System.Windows.Media;

using UI;

namespace Lab_3_4.Converters
{
    class GroupToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Contract.Requires(value is int);
            Contract.Requires(targetType == typeof(Brush));
            
            int groupID = (int)value;
            if (groupID == -1)
            {
                return Brushes.White;
            }

            return (object)ColorBox.GetNext(groupID);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
