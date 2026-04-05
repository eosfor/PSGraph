using QuikGraph;
using QuikGraph.Algorithms;
using System.Dynamic;

namespace PSGraph.Model
{
    public class PsBidirectionalGraph : BidirectionalGraph<PSVertex, PSEdge>
    {
        public IDictionary<string, object?> RenderProperties { get; set; } = new ExpandoObject();

        // A: default now disallows parallel edges
        public PsBidirectionalGraph() : this(false) { }

        public PsBidirectionalGraph(bool allowParallelEdges = false) : base(allowParallelEdges) { }

        public PsBidirectionalGraph(PsBidirectionalGraph g) : base(g)
        {
            RenderProperties = CloneProperties(g.RenderProperties);
        }

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

        private static IDictionary<string, object?> CloneProperties(IDictionary<string, object?> src)
        {
            var expando = new ExpandoObject();
            var dst = (IDictionary<string, object?>)expando;
            foreach (var kv in src)
            {
                dst[kv.Key] = kv.Value is ICloneable c ? c.Clone() : kv.Value;
            }
            return dst;
        }
    }
}
