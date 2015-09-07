using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab_1
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty CodeTextProperty = DependencyProperty.Register("CodeText", typeof(string), typeof(CodeEditor));

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        public string CodeText
        {
            get
            {
                return (string)GetValue(CodeTextProperty);
            }
            set
            {
                SetValue(CodeTextProperty, value);
                this.OnPropertyChanged("CodeText");
            }
        }

        public CodeEditor()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Code_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.LineNumbers.Text = string.Join("\n", Enumerable.Range(1, this.Code.LineCount));
        }

        private void Code_Scrolled(object sender, ScrollChangedEventArgs e)
        {
            int line = (e.VerticalChange > 0) ? this.Code.GetLastVisibleLineIndex() : this.Code.GetFirstVisibleLineIndex();

            if (this.LineNumbers.LineCount > line)
                this.LineNumbers.ScrollToLine(line);
        }
    }
}
