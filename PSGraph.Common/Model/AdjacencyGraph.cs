using QuikGraph;

namespace PSGraph.Model
{
    public class PsAdjacencyGraph : AdjacencyGraph<PSVertex, PSEdge>
    {
        public PsAdjacencyGraph(bool allowParallelEdges = false) : base(allowParallelEdges)
        {
        }
        
        public PsAdjacencyGraph() : base()
        {
            
        }
    }
}
