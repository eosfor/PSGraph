using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Model
{
    public class PSBidirectionalGraph : BidirectionalGraph<PSVertex, PSEdge>
    {
        public PSBidirectionalGraph(bool allowParallelEdges = false) : base(allowParallelEdges)
        {
        }
    }
}
