using GraphSharp.Controls;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph
{
    public class PocGraphLayout : GraphLayout<PocVertex, PocEdge, PocGraph> { }
    public class PocGraphLayout2 : GraphLayout<object, STaggedEdge<object, object>, BidirectionalGraph<object, STaggedEdge<object, object>>> { }

    public class PocGraph : BidirectionalGraph<PocVertex, PocEdge>
    {
        public PocGraph() { }
        public PocGraph(AdjacencyGraph<object, STaggedEdge<object, object>> graph)
        {
            for (int i=0; i < graph.VertexCount; i++)
            {
                var v = graph.Vertices.ElementAt(i);
                var vertex = new PocVertex(i.ToString(), graph.Vertices.ElementAt(i));
                this.AddVertex(vertex);
            }
        }
    }

    [DebuggerDisplay("{ID}")]
    public class PocVertex
    {
        public PocVertex(string id, object data)
        {
            ID = id;
            Data = data;
        }

        public PocVertex(string id)
        {
            ID = id;
        }

        public string ID { get; private set; }
        public object Data { get; private set; }


        public override string ToString()
        {
            return ID;
        }
    }

    [DebuggerDisplay("{Source.ID} -> {Target.ID}")]
    public class PocEdge : Edge<PocVertex>
    {
        public string ID { get; private set; }

        public PocEdge(string id, PocVertex source, PocVertex target)
            : base(source, target)
        {
            ID = id;
        }
    }

}
