using QuikGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Model
{
    public class PSVertex
    {
        public string Label;
        public string Name => Label;
        public GraphvizVertex GVertexParameters = new GraphvizVertex();
        public object? OriginalObject;
        public List<object>? Metadata;

        public PSVertex(string label)
        {
            Label = label;
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
    }
}
