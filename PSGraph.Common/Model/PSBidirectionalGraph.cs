using QuikGraph;
using QuikGraph.Algorithms;

namespace PSGraph.Model
{
    public class PsBidirectionalGraph : BidirectionalGraph<PSVertex, PSEdge>
    {
        // A: default now disallows parallel edges
        public PsBidirectionalGraph() : this(false) { }

        public PsBidirectionalGraph(bool allowParallelEdges = false) : base(allowParallelEdges) { }

        public PsBidirectionalGraph(PsBidirectionalGraph g) : base(g) { }

        // Strongly-typed clone that preserves the derived type
        public new PsBidirectionalGraph Clone()
        {
            return new PsBidirectionalGraph(this);
        }

        public bool IsDag => this.IsDirectedAcyclicGraph();

        public IEnumerable<PSVertex> Roots => this.Roots();

        // B: explicit duplicate prevention + vertex reuse by label
        public override bool AddEdge(PSEdge edge)
        {
            if (edge is null) return false;

            var source = Vertices.FirstOrDefault(v => v == edge.Source) ?? edge.Source;
            var target = Vertices.FirstOrDefault(v => v == edge.Target) ?? edge.Target;

            if (!Vertices.Contains(source)) AddVertex(source);
            if (!Vertices.Contains(target)) AddVertex(target);

            var toAdd = (ReferenceEquals(source, edge.Source) && ReferenceEquals(target, edge.Target))
                ? edge
                : new PSEdge(source, target, edge.Tag);

            return base.AddEdge(toAdd);
        }

        public override bool AddVerticesAndEdge(PSEdge edge)
        {
            if (edge is null) return false;
            // Delegate to AddEdge after ensuring canonical vertices
            return AddEdge(edge);
        }
    }
}
