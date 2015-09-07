using Simulation;

using System;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.Converters
{
    public class CellToColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var grid = values[0] as DataGrid;
            var cell = values[1] as DataGridCell;

            if (grid == null || cell == null)
                return Brushes.White;

            var row = (grid.ItemsSource as IList).IndexOf(cell.DataContext);
            var cellValue = (cell.DataContext as Step).ProcessorOperations[cell.Column.DisplayIndex - 1];

            return string.IsNullOrEmpty(cellValue) ? Brushes.White : ColorBox.GetNext(row);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
