using UI;
using Transformation;
using Language.Analyzers;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace Lab_3_4.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string expression;

        public RelayCommand RemoveParenthesesCommand { get; private set; }
        public RelayCommand ApplyCommutativityCommand { get; private set; }
        public RelayCommand<EquivalentForm> CopyListItemCommand { get; private set; }

        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
                RaisePropertyChanged("Expression");

                ApplyCommutativityCommand.RaiseCanExecuteChanged();
                RemoveParenthesesCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<EquivalentForm> EquForms { get; private set; }

        public MainViewModel()
        {
            this.RemoveParenthesesCommand = new RelayCommand(this.RemoveParentheses, this.CanGetEquivalentForms);
            this.ApplyCommutativityCommand = new RelayCommand(this.ApplyCommutativity, this.CanGetEquivalentForms);
            this.CopyListItemCommand = new RelayCommand<EquivalentForm>(this.CopyListItem);

            this.Expression = "";
            this.EquForms = new ObservableCollection<EquivalentForm>();
        }

        private bool CanGetEquivalentForms()
        {
            return !string.IsNullOrWhiteSpace(this.Expression);
        }

        private void CopyListItem(EquivalentForm item)
        {
            Clipboard.Clear();
            Clipboard.SetText(item.Expression);
        }

        private async void RemoveParentheses()
        {
            var forms = await Task.Factory.StartNew(() =>
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer();
                lexer.Feed(Expression);

                return new ParenthesesRemoval(lexer.Analyze()).GetEquivalentForms();
            });

            EquForms.Clear();
            foreach (var form in forms)
            {
                EquForms.Add(form);
            }
        }

        private async void ApplyCommutativity()
        {
            var forms = await Task.Factory.StartNew(() =>
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer();
                lexer.Feed(Expression);

                return new Commutativity(lexer.Analyze()).GetEquivalentForms();
            });

            /* Shuffle equivalent from groups' colors */
            ColorBox.Shuffle();
            EquForms.Clear();

            foreach (var form in forms)
            {
                EquForms.Add(form);
            }
        }
    }
}