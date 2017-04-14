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
        public string ResourceID = null;
    }

    public class PSGraphVertexComparer : EqualityComparer<PSGraphVertex>
    {
        public override bool Equals(PSGraphVertex x, PSGraphVertex y)
        {
            return (x.Label == y.Label) && (x.ResourceID == y.ResourceID);
        }

        public override int GetHashCode(PSGraphVertex x)
        {
            return x.Label.GetHashCode();
        }
    }
}
