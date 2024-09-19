using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
