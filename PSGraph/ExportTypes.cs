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

    public class PSGraphVertexComparer : EqualityComparer<PSGraphVertex>
    {
        public override bool Equals(PSGraphVertex left, PSGraphVertex right)
        {
            if (left.GetType() != right.GetType())
            {
                return false;
            }

            return left.Equals(right);
        }

        public override int GetHashCode(PSGraphVertex x)
        {
            return x.GetHashCode();
        }
    }
}
