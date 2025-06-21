using QuikGraph.Graphviz.Dot;

namespace PSGraph.Model
{
    public class PSVertex: IComparable<PSVertex>
    {
        private string label;
        public string Name => Label;

        public string Label { get => label; set => SetLabel(value); }


        public GraphvizVertex GVertexParameters = new GraphvizVertex();
        public object? OriginalObject;
        public List<object>? Metadata;


        private void SetLabel(string value)
        {
            label = value;
            GVertexParameters.Label = value;
            //throw new NotImplementedException();
        }

        public PSVertex(string label)
        {
            Label = label;
        }

        public PSVertex(PSVertex v)
        {
            string l = new string(v.label.ToCharArray()); //copy?
            var copy = new PSVertex(l);
        }

        public PSVertex(string label, object source)
        {
            Label = label;
            OriginalObject = source;
        }

        //public static implicit operator PSVertex(object obj)
        //{
        //    return new PSVertex(obj.ToString());
        //}

        public int CompareTo(PSVertex? other)
        {
            return this.Label.CompareTo(other.Label);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType())
            {
                return false;
            }
            return Label == ((PSVertex)obj).Label;
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        public override string ToString()
        {
            return Label;
        }

        // private class SortByDegree: IComparer<PSVertex>
        // {
        //     private readonly PsBidirectionalGraph _graph;
        //
        //     public int Compare(PSVertex? x, PSVertex? y)
        //     {
        //         return _graph.Degree(x) - _graph.Degree(y);
        //     }
        //
        //     public SortByDegree(PsBidirectionalGraph g)
        //     {
        //         _graph = g;
        //     }
        // }
    }
}
