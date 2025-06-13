using QuikGraph;

namespace PSGraph.Model
{
    public class PsBidirectionalGraph : BidirectionalGraph<PSVertex, PSEdge>
    {
        public PsBidirectionalGraph(bool allowParallelEdges = false) : base(allowParallelEdges)
        {
        }
        
        public PsBidirectionalGraph(PsBidirectionalGraph g) : base(g)
        {
            
        }
    }
}
