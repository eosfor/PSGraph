using QuickGraph.Graphviz.Dot;
using System.Collections.Generic;

namespace PSGraph
{
    public enum ExportTypes
    {
        Graphviz,
        GraphML
    }

    public abstract class PSGraphVertex : GraphvizVertex
    {
        public override int GetHashCode()
        {
            return UniqueKey.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            return UniqueKey == ((PSGraphVertex)obj).UniqueKey;
        }

        /// <summary>
        /// Vertex unique key, eg Label + ResourceId
        /// </summary>
        public abstract string UniqueKey { get; }
    }
}
