using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using QuickGraph;
using System.Diagnostics;
using System.Globalization;
using GraphSharp.Controls;
using System.IO;
using System.Xml;
using QuickGraph.Graphviz;
using QuickGraph.Serialization;
using System.ComponentModel;

namespace PSGraph
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GraphLayoutWindow : Window
    {
        private GraphVewModel gw;

        public GraphLayoutWindow()
        {
            InitializeComponent();

        }

        public GraphLayoutWindow(dynamic graph)
        {
            //gw = new GraphVewModel(graph);
            
            InitializeComponent();
            DataContext = graph;
        }
    }
}
