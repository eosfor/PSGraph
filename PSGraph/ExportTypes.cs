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
        //requredt to avoid cross-type comparisons in mixed dictionary
        public bool IsTypeEqual(PSGraphVertex left)
        {
            return this.GetType() == left.GetType();
        }
    }
}
