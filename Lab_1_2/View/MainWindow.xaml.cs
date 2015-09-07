using System.Windows;

namespace Lab_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (this.Tabs.SelectedIndex)
            {
                case 2:
                    this.TreeGraphZoomCtrl.ZoomToFill();
                    break;

                case 3:
                    this.ParallelTreeGraphZoomCtrl.ZoomToFill();
                    break;
            }            
        }
    }
}
