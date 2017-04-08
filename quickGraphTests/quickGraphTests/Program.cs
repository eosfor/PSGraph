using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Graphviz.Dot;

namespace quickGraphTests
{
    class Program
    {
        class baseVertexComparer  : EqualityComparer<GraphvizVertex>
        {
            public override bool Equals(GraphvizVertex x, GraphvizVertex y)
            {
                return (x.Label == y.Label);
            }

            public override int GetHashCode(GraphvizVertex x)
            {
                return x.Label.Length;
            }
        }
        static void Main(string[] args)
        {
            var eqComparer = new baseVertexComparer();
            var newGraph = new AdjacencyGraph<GraphvizVertex, STaggedEdge<GraphvizVertex, int>>(false,-1,-1, eqComparer);
            var v1 = new GraphvizVertex{ Label = "vnet1" };
            var v2 = new GraphvizVertex { Label = "vnet1" };
            var v3 = new GraphvizVertex { Label = "vnet2" };

            Console.WriteLine ("Adding V1 first time: " + newGraph.AddVertex(v1));
            Console.WriteLine("Adding V1 second time: " + newGraph.AddVertex(v1));
            Console.WriteLine("Adding V2 fisrt time: " + newGraph.AddVertex(v2));
            Console.WriteLine("Adding V3 fist time: " + newGraph.AddVertex(v3));

            Console.ReadLine();

        }
    }
}
