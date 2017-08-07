using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GraphSharp.Controls;
using QuickGraph;
using GraphSharp.Algorithms.Layout;

namespace PSGraph
{
    class GraphVewModel : INotifyPropertyChanged
    {
        private string layoutAlgorithmType;
        private dynamic _graph;
        private List<String> layoutAlgorithmTypes = new List<string>();
        private dynamic _graphLayout;


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
                _graphLayout.LayoutAlgorithmType = layoutAlgorithmType;
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

            Type graphType = inGraph.GetType();
            Type[] graphGenericArgs = graphType.GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];

            Type graphLayoutType = typeof(GraphLayout<,,>);
            var gcType = graphLayoutType.MakeGenericType(vertexType, edgeType, graphType);
            _graphLayout = Activator.CreateInstance(gcType);

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
            LayoutAlgorithmType = "Tree";



            _graphLayout.Graph = Graph;
            _graphLayout.LayoutAlgorithmType = LayoutAlgorithmType;
            //_graphLayout.OverlapRemovalAlgorithmType = "FSA";   //strange thing here
            _graphLayout.HighlightAlgorithmType = "Simple";
            _graphLayout.LayoutParameters.WidthPerHeight = 10.0;
            _graphLayout.LayoutParameters.OptimizeWidthAndHeight = true;
            _graphLayout.LayoutParameters.Direction = LayoutDirection.LeftToRight;

            //GraphLayout<object, STaggedEdge<object, object>, BidirectionalGraph<object, STaggedEdge<object, object>>> _gl = new GraphLayout<object, STaggedEdge<object, object>, BidirectionalGraph<object, STaggedEdge<object, object>>>();
            //_gl.LayoutAlgorithmType = "Tree";

            CurrentGraph = _graphLayout;
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
                _graphLayout.Relayout();
            }
        }

        #endregion
    }




}
