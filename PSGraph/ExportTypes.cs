using QuickGraph.Graphviz.Dot;
using System.Collections.Generic;

namespace PSGraph
{
    public enum ExportTypes
    {
        Graphviz
    }

    public class PSGraphVertex : GraphvizVertex
    {

    }

    public class PSGraphVertexComparer : EqualityComparer<GraphvizVertex>
    {
        public override bool Equals(GraphvizVertex x, GraphvizVertex y)
        {
            return x.Label == y.Label;
        }

        public override int GetHashCode(GraphvizVertex x)
        {
            return x.Label.GetHashCode();
        }
    }
}
