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

        public override bool AddEdge(PSEdge edge)
        {
            var s = Vertices.Contains(edge.Source) ? Vertices.First(v => v == edge.Source) : edge.Source;
            var t = Vertices.Contains(edge.Target) ? Vertices.First(v => v == edge.Target) : edge.Target;

            if (!Vertices.Contains(s))
                AddVertex(s);
            if (!Vertices.Contains(t))
                AddVertex(t);

            var e = new PSEdge(s, t, edge.Tag);
            return base.AddEdge(e);
        }

        public override bool AddVerticesAndEdge(PSEdge edge)
        {
            PSVertex s = edge.Source;
            PSVertex t = edge.Target;

            if (!Vertices.Contains(s))
                AddVertex(s);
            if (!Vertices.Contains(t))
                AddVertex(t);

            var e = new PSEdge(
                Vertices.First(v => v == s),
                Vertices.First(v => v == t),
                edge.Tag
            );

            return base.AddEdge(e);
        }


        private bool Exists(PSVertex vertex)
        {
            return this.Vertices.Contains(vertex);
        }
    }
}
