using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lab_5.Converters
{
    class TreeItemToLevel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var treeItem = value as TreeViewItem;
            if (treeItem == null)
            {
                return 0;
            }

            int depth = 0;
            var current = treeItem;

            while (current is TreeViewItem)
            {
                current = current.Parent as TreeViewItem;
                depth++;
            }
            return depth;
        }
    }
}
