using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GraphSharp.Controls;
using QuickGraph;

namespace PSGraph
{
    class GraphVewModel : INotifyPropertyChanged
    {
        private string layoutAlgorithmType;
        private dynamic _graph;
        private List<String> layoutAlgorithmTypes = new List<string>();


        #region Public Properties

        public dynamic CurrentGraph { get; private set; }

        public List<String> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }
        }

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                NotifyPropertyChanged("LayoutAlgorithmType");
            }
        }

        public dynamic Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        #endregion

        public GraphVewModel(dynamic inGraph)
        {
            Graph = inGraph;

            //Add Layout Algorithm Types
            layoutAlgorithmTypes.Add("BoundedFR");
            layoutAlgorithmTypes.Add("Circular");
            layoutAlgorithmTypes.Add("CompoundFDP");
            layoutAlgorithmTypes.Add("EfficientSugiyama");
            layoutAlgorithmTypes.Add("FR");
            layoutAlgorithmTypes.Add("ISOM");
            layoutAlgorithmTypes.Add("KK");
            layoutAlgorithmTypes.Add("LinLog");
            layoutAlgorithmTypes.Add("Tree");

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "EfficientSugiyama";

            Type graphType = inGraph.GetType();
            Type[] graphGenericArgs = graphType.GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];

            Type graphLayoutType = typeof(GraphLayout<,,>);
            var gcType = graphLayoutType.MakeGenericType(vertexType, edgeType, graphType);
            dynamic gc = Activator.CreateInstance(gcType);

            gc.Graph = Graph;
            gc.LayoutAlgorithmType = LayoutAlgorithmType;
            //gc.OverlapRemovalAlgorithmType = "FSA";
            gc.HighlightAlgorithmType = "Simple";

            CurrentGraph = gc;
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }




}
