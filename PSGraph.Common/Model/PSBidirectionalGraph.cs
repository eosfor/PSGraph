using QuikGraph;
using QuikGraph.Algorithms;
using System.Dynamic;

namespace PSGraph.Model
{
    public class PsBidirectionalGraph : BidirectionalGraph<PSVertex, PSEdge>
    {
        public IDictionary<string, object?> RenderProperties { get; set; } = new ExpandoObject();
        public bool UseNonUniqueLabels { get; }

        // A: default now disallows parallel edges
        public PsBidirectionalGraph() : this(false) { }

        public PsBidirectionalGraph(bool allowParallelEdges = false, bool useNonUniqueLabels = false) : base(allowParallelEdges)
        {
            UseNonUniqueLabels = useNonUniqueLabels;
        }

        public PsBidirectionalGraph(PsBidirectionalGraph g) : base(g)
        {
            UseNonUniqueLabels = g.UseNonUniqueLabels;
            RenderProperties = CloneProperties(g.RenderProperties);
        }

        // Strongly-typed clone that preserves the derived type
        public new PsBidirectionalGraph Clone()
        {
            return new PsBidirectionalGraph(this);
        }

        public bool IsDag => this.IsDirectedAcyclicGraph();

        public IEnumerable<PSVertex> Roots => this.Roots();

        public PSVertex AddOrGetVertex(PSVertex vertex)
        {
            if (vertex is null)
            {
                throw new ArgumentNullException(nameof(vertex));
            }

            if (UseNonUniqueLabels)
            {
                var existingUnique = Vertices.FirstOrDefault(v => ReferenceEquals(v, vertex) || v == vertex);
                if (existingUnique is not null)
                {
                    return existingUnique;
                }

                vertex.EnsureUniqueIdentity();
                base.AddVertex(vertex);
                return vertex;
            }

            var existing = Vertices.FirstOrDefault(v => v == vertex);
            if (existing is not null)
            {
                return existing;
            }

            base.AddVertex(vertex);
            return vertex;
        }

        public override bool AddVertex(PSVertex vertex)
        {
            if (vertex is null)
            {
                return false;
            }

            var existing = UseNonUniqueLabels
                ? Vertices.FirstOrDefault(v => ReferenceEquals(v, vertex) || v == vertex)
                : Vertices.FirstOrDefault(v => v == vertex);

            if (existing is not null)
            {
                return false;
            }

            if (UseNonUniqueLabels)
            {
                vertex.EnsureUniqueIdentity();
            }

            return base.AddVertex(vertex);
        }

        public override bool AddEdge(PSEdge edge)
        {
            if (edge is null) return false;

            var source = AddOrGetVertex(edge.Source);
            var target = AddOrGetVertex(edge.Target);

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
