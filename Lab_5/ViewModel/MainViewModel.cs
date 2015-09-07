using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using GraphX;
using GraphX.Controls;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Tree;

using Lab_1.Graph;
using Language.Analyzers;
using Language.Tokens;

using UI;
using Simulation;
using Parallel.Tree;
using Transformation;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Media;

namespace Lab_5.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private fields

        private const int DefaultProcessorCount = 4;

        private string expression;
        private int processorCount;
        private SimulationResult simulationResult;

        private int selectedTabIndex;
        private bool isTreeExpanded;

        private string selectedEquForm;
        private MatrixSystem matrixSystem;
        private int parallelTreeHeight;

        private GraphLogicCore parallelTreeGraph;
        private ZoomControl ParallelTreeGraphZoomCtrl;
        private TreeGraphArea ParallelTreeGraphArea;

        #endregion

        #region Binding properties

        public string SelectedEquForm
        {
            get
            {
                return this.selectedEquForm;
            }
            set
            {
                this.selectedEquForm = value;
                base.RaisePropertyChanged("SelectedEquForm");
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return this.selectedTabIndex;
            }
            set
            {
                this.selectedTabIndex = value;
                base.RaisePropertyChanged("SelectedTabIndex");
            }
        }

        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
                base.RaisePropertyChanged("Expression");
            }
        }

        public int ProcessorCount
        {
            get
            {
                return this.processorCount;
            }
            set
            {
                this.processorCount = value;
                this.matrixSystem.ProcessorCount = value;

                this.SimulationResult = null;
                base.RaisePropertyChanged("ProcessorCount");

                if (this.SimulateCommand.CanExecute(null))
                    this.SimulateCommand.Execute(null);
            }
        }

        public bool IsTreeExpanded
        {
            get
            {
                return this.isTreeExpanded;
            }
            set
            {
                this.isTreeExpanded = value;
                base.RaisePropertyChanged("IsTreeExpanded");
            }
        }

        public SimulationResult SimulationResult
        {
            get
            {
                return this.simulationResult;
            }
            set
            {
                this.simulationResult = value;
                base.RaisePropertyChanged("SimulationResult");
            }
        }

        public ObservableCollection<string> ParenthesesRemovalEquForms { get; private set; }
        public ObservableCollection<string> ComutativityEquForms { get; private set; }
        public ObservableCollection<string> OriginalForms { get; private set; }

        public GraphLogicCore ParallelTreeGraph
        {
            get
            {
                return this.parallelTreeGraph;
            }
            set
            {
                this.parallelTreeGraph = value;
                base.RaisePropertyChanged("ParallelTreeGraph");
            }
        }

        public int ParallelTreeHeight
        {
            get
            {
                return this.parallelTreeHeight;
            }
            set
            {
                this.parallelTreeHeight = value;
                base.RaisePropertyChanged("ParallelTreeHeight");
            }
        }

        #endregion

        #region Commands

        public RelayCommand<string> CopyTreeViewItemCommand { get; private set; }
        public RelayCommand<object> ItemSelectedCommand { get; private set; }

        public RelayCommand GetEquFormsCommand { get; private set; }
        public RelayCommand SimulateCommand { get; private set; }
        public RelayCommand ViewLoaded { get; private set; }

        #endregion

        public MainViewModel()
        {
            this.ViewLoaded = new RelayCommand(this.FindControls);
            this.SimulateCommand = new RelayCommand(this.Simulate, this.CanProcessTreeItem);
            this.GetEquFormsCommand = new RelayCommand(this.GetEquForms, this.CanGetEquForms);

            this.ItemSelectedCommand = new RelayCommand<object>(this.ItemSelected);
            this.CopyTreeViewItemCommand = new RelayCommand<string>(this.CopyTreeViewItem);

            this.matrixSystem = new MatrixSystem(DefaultProcessorCount);

            this.SelectedTabIndex = 1;
            this.ProcessorCount = DefaultProcessorCount;
            this.IsTreeExpanded = false;

            this.OriginalForms = new ObservableCollection<string>();
            this.ComutativityEquForms = new ObservableCollection<string>();
            this.ParenthesesRemovalEquForms = new ObservableCollection<string>();
        }

        #region Private methods

        private bool CanProcessTreeItem()
        {
            return !string.IsNullOrEmpty(this.SelectedEquForm);
        }

        private bool CanGetEquForms()
        {
            return this.Expression != "";
        }

        private void CopyTreeViewItem(string item)
        {
            Clipboard.Clear();
            Clipboard.SetText(item);
        }

        private void ItemSelected(object item)
        {
            this.SelectedEquForm = (item as string) ?? "";
        }

        private void Simulate()
        {
            this.SelectedTabIndex = 0;

            LexicalAnalyzer lexer = new LexicalAnalyzer();
            lexer.Feed(this.SelectedEquForm);

            var tokens = lexer.Analyze();
            var tree = Tree.FromTokens(tokens);

            tree.Balance();

            this.ParallelTreeHeight = tree.Height;
            this.SimulationResult = this.matrixSystem.SimulateCalculation(tree);

            /* Create the parallel tree graph structure */
            var graph = Visualization.GetTreeGraph(tree);
            this.ParallelTreeGraph = new GraphLogicCore { Graph = graph, DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree };
            
            /* Configure layout parameters */
            this.ParallelTreeGraph.DefaultLayoutAlgorithmParams = this.ParallelTreeGraph.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Tree);
            var layoutParams = this.ParallelTreeGraph.DefaultLayoutAlgorithmParams as SimpleTreeLayoutParameters;
            layoutParams.SpanningTreeGeneration = SpanningTreeGeneration.BFS;

            /* Set vertex colors and display the graph */
            this.ParallelTreeGraphArea.GenerateGraph(true);

            /* Set vertex colors */
            for (int stepIndex = 0; stepIndex < this.SimulationResult.Steps.Count; stepIndex++)
            {
                foreach (var node in this.SimulationResult.Steps[stepIndex].GraphNodes)
                {
                    var vertex = ParallelTreeGraphArea.VertexList.Keys.First(v => v.ID == node.GetHashCode());
                    var control = ParallelTreeGraphArea.VertexList[vertex];
                    
                    control.Background = ColorBox.GetNext(stepIndex);
                    control.BorderThickness = new Thickness(1);
                    control.BorderBrush = Brushes.Black;
                }
            }
            this.ParallelTreeGraphZoomCtrl.ZoomToFill();
        }

        private async void GetEquForms()
        {
            var cef = await Task.Factory.StartNew(() =>
            {            
                LexicalAnalyzer lexer = new LexicalAnalyzer();
                lexer.Feed(Expression);
                var tokens = lexer.Analyze();
                return new Commutativity(tokens).GetEquivalentForms();
            });

            var pref = await Task.Factory.StartNew(() =>
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer();
                lexer.Feed(Expression);
                var tokens = lexer.Analyze();
                return new ParenthesesRemoval(tokens).GetEquivalentForms();
            });

            this.OriginalForms.Clear();
            this.ComutativityEquForms.Clear();
            this.ParenthesesRemovalEquForms.Clear();

            LexicalAnalyzer lex = new LexicalAnalyzer();
            lex.Feed(Expression);
            this.OriginalForms.Add(Token.Concat(lex.Analyze()));

            foreach (var form in cef)
            {
                if (!form.IsMinor)
                {
                    this.ComutativityEquForms.Add(form.Expression);
                }
            }

            foreach (var form in pref)
            {
                if (!form.IsMinor)
                {
                    this.ParenthesesRemovalEquForms.Add(form.Expression);
                }
            }

            this.IsTreeExpanded = true;
        }

        private void FindControls()
        {
            ParallelTreeGraphArea = App.Current.MainWindow.FindName("ParallelTreeGraphArea") as TreeGraphArea;
            ParallelTreeGraphZoomCtrl = App.Current.MainWindow.FindName("ParallelTreeGraphZoomCtrl") as ZoomControl;

            ParallelTreeGraphArea.ShowAllEdgesArrows(false);
            ParallelTreeGraphZoomCtrl.ZoomDeltaMultiplier = 20;
            ZoomControl.SetViewFinderVisibility(ParallelTreeGraphZoomCtrl, Visibility.Hidden);
        }

        #endregion
    }
}