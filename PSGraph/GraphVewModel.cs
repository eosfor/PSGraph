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
        private BidirectionalGraph<object, STaggedEdge<object, object>> _graph;
        private List<String> layoutAlgorithmTypes = new List<string>();

        #region Public Properties

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

        public BidirectionalGraph<object, STaggedEdge<object, object>> Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        #endregion

        public GraphVewModel(BidirectionalGraph<object, STaggedEdge<object, object>> inGraph)
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
