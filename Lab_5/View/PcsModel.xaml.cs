using UI;
using Simulation;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lab_5.View
{
    public partial class PcsModel : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty SimResultProperty =
            DependencyProperty.Register("SimResult", typeof(SimulationResult), typeof(PcsModel), new PropertyMetadata(SimResultChanged));

        public static readonly DependencyProperty ProcessorCountProperty =
            DependencyProperty.Register("ProcessorCount", typeof(int), typeof(PcsModel), new PropertyMetadata(0, ProcessorCountChanged));

        private static void SimResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PcsModel).UpdateGrid();
        }

        private static void ProcessorCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorBox.Shuffle();
            (d as PcsModel).CreateRemoveProcessorColumns();
        }

        public SimulationResult SimResult
        {
            get { return (SimulationResult)GetValue(SimResultProperty); }
            set { SetValue(SimResultProperty, value); }
        }

        public int ProcessorCount
        {
            get { return (int)GetValue(ProcessorCountProperty); }
            set { SetValue(ProcessorCountProperty, value); }
        }

        #endregion

        public PcsModel()
        {
            InitializeComponent();
        }

        #region Private methods

        private void CreateRemoveProcessorColumns()
        {
            int procCount = ProcessorCount;
            int columnsNeeded = procCount - (ModelGrid.Columns.Count - 2);

            /* Remove last (-columnsNeeded) processor columns */
            if (columnsNeeded < 0)
            {
                for (int k = columnsNeeded; k < 0; k++)
                {
                    ModelGrid.Columns.RemoveAt(ModelGrid.Columns.Count - 2);
                }
            }

            /* Add columnsNeeded more processor columns otherwise */
            else
            {
                for (int k = 0; k < columnsNeeded; k++)
                {
                    int currentProc = ModelGrid.Columns.Count - 1;
                    ModelGrid.Columns.Insert(currentProc, new DataGridTextColumn
                    {
                        Header = string.Format("Процесор {0}", currentProc),
                        Binding = new Binding(string.Format("ProcessorOperations[{0}]", currentProc - 1)),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                        CellStyle = this.Resources["ProcessorCell"] as Style
                    });
                }
            }
        }

        private void UpdateGrid()
        {
            this.ModelGrid.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget();
            this.TBTotalTime.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }

        private void ModelGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TBTotalTime.Width = TimeColumn.ActualWidth;
        }

        #endregion
    }
}
