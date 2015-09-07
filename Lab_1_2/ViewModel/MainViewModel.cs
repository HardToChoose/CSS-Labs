using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Lab_1.Graph;

using Language;
using Language.Tokens;
using Language.Analyzers;

using Parallel.Tree;
using GraphX;
using GraphX.Controls;
using System;
using System.Windows.Shapes;

namespace Lab_1.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private fields

        private bool findAllErrors = false;
        private int activeTabIndex = 0;
        private string document = "";

        private Tree tree = null;
        private Tree parallelTree = null;

        private GraphLogicCore treeGraph = null;
        private GraphLogicCore parallelTreeGraph = null;

        private OpenFileDialog openFile = new OpenFileDialog();

        #endregion

        #region Binded properties

        public Tree Tree
        {
            get
            {
                return this.tree;
            }
            set
            {
                this.tree = value;
                base.RaisePropertyChanged("Tree");
            }
        }

        public Tree ParallelTree
        {
            get
            {
                return this.parallelTree;
            }
            set
            {
                this.parallelTree = value;
                base.RaisePropertyChanged("ParallelTree");
            }
        }

        public GraphLogicCore TreeGraph
        {
            get
            {
                return this.treeGraph;
            }
            set
            {
                this.treeGraph = value;
                base.RaisePropertyChanged("TreeGraph");
            }
        }

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

        public string Document
        {
            get
            {
                return this.document;
            }
            set
            {
                this.document = value;
                base.RaisePropertyChanged("Document");
            }
        }

        public int ActiveTabIndex
        {
            get
            {
                return this.activeTabIndex;
            }
            set
            {
                if (value == 3)
                {
                    BuildParallelTree();
                }

                this.activeTabIndex = value;
                base.RaisePropertyChanged("ActiveTabIndex");
            }
        }

        public bool FindAllErrors
        {
            get
            {
                return this.findAllErrors;
            }
            set
            {
                if (this.findAllErrors != value)
                {
                    this.findAllErrors = value;
                    base.RaisePropertyChanged("FindAllErrors");
                }
            }
        }

        public ObservableCollection<Error> ParsingErrors { get; private set; }

        #endregion

        public ICommand AnalyzeCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }

        public MainViewModel()
        {
            this.ParsingErrors = new ObservableCollection<Error>();
            this.AnalyzeCommand = new RelayCommand(this.Analyze);
            this.OpenCommand = new RelayCommand(this.LoadFile);

            this.ViewLoaded = new RelayCommand(this.FindControls);
        }

        private void LoadFile()
        {
            if (this.openFile.ShowDialog() == DialogResult.OK)
            {
                this.activeTabIndex = 1;
                using (var reader = new StreamReader(this.openFile.FileName))
                {
                    this.Document = reader.ReadToEnd();
                }
            }
        }

        private IList<Token> Parse(string code, bool findAll)
        {
            LexicalAnalyzer lex = new LexicalAnalyzer();
            SyntacticalAnalyzer syn = new SyntacticalAnalyzer();
            SemanticalAnalyzer sem = new SemanticalAnalyzer();

            lex.Feed(code);
            var tokens = lex.Analyze();

            if (ErrorLogger.Errors.Count() == 0)
            {
                syn.Analyze(tokens, findAll);
            }
            return tokens;
        }

        private TreeGraph BuildTree(IList<Token> tokens)
        {
            this.Tree = Tree.FromTokens(tokens);
            return Visualization.GetTreeGraph(this.Tree);
        }

        private async void Analyze()
        {
            ErrorLogger.ClearLog();
            this.ParsingErrors.Clear();

            /* Parse the input expression into tokens */
            var tokens = await Task.Factory.StartNew(() => this.Parse(this.Document, this.FindAllErrors));
            
            /* Add parsing errors into the table */
            foreach (var error in ErrorLogger.Errors)
                this.ParsingErrors.Add(error);

            /* Build expression tree and show its graph */
            if (ErrorLogger.Errors.Count() == 0)
            {
                this.ActiveTabIndex = 2;
                var graph = await Task.Factory.StartNew<TreeGraph>(() => this.BuildTree(tokens));

                this.TreeGraph = new GraphLogicCore { Graph = graph, DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree };

                /* MVVM's BEING RAPED!!!11 */
                this.TreeGraphArea.GenerateGraph(true);
                this.TreeGraphZoomCtrl.ZoomToFill();
            }
            else
                this.ActiveTabIndex = 1;
        }

        private async void BuildParallelTree()
        {
            if (tree == null)
                return;

            /* Build parallel expression tree */
            var graph = await Task.Factory.StartNew<TreeGraph>(() =>
            {
                this.ParallelTree = tree.Clone() as Tree;
                this.ParallelTree.Balance();

                return Visualization.GetTreeGraph(this.ParallelTree);
            });

            /* Display the parallel tree graph */
            this.ParallelTreeGraph = new GraphLogicCore { Graph = graph, DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree };

            /* Oops I did it again */
            this.ParallelTreeGraphArea.GenerateGraph(true);
            this.ParallelTreeGraphZoomCtrl.ZoomToFill();
        }

        /* Далі починається збочинство */
        private ZoomControl TreeGraphZoomCtrl;
        private ZoomControl ParallelTreeGraphZoomCtrl;

        private TreeGraphArea TreeGraphArea;
        private TreeGraphArea ParallelTreeGraphArea;

        public ICommand ViewLoaded { get; private set; }

        private void FindControls()
        {
            TreeGraphArea = App.Current.MainWindow.FindName("TreeGraphArea") as TreeGraphArea;
            ParallelTreeGraphArea = App.Current.MainWindow.FindName("ParallelTreeGraphArea") as TreeGraphArea;

            TreeGraphZoomCtrl = App.Current.MainWindow.FindName("TreeGraphZoomCtrl") as ZoomControl;
            ParallelTreeGraphZoomCtrl = App.Current.MainWindow.FindName("ParallelTreeGraphZoomCtrl") as ZoomControl;

            TreeGraphZoomCtrl.ZoomDeltaMultiplier = 20;
            ParallelTreeGraphZoomCtrl.ZoomDeltaMultiplier = 20;

            ZoomControl.SetViewFinderVisibility(TreeGraphZoomCtrl, Visibility.Hidden);
            ZoomControl.SetViewFinderVisibility(ParallelTreeGraphZoomCtrl, Visibility.Hidden);
        }
    }
}