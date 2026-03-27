namespace PSGraph.Model;

public sealed record GraphView(IReadOnlyList<GraphViewNode> Nodes, IReadOnlyList<GraphViewEdge> Edges);

public sealed record GraphViewNode(
    string Id,
    string Label,
    string? TypeName,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record GraphViewEdge(
    string SourceId,
    string TargetId,
    string? Label,
    int Weight);

public static class GraphViewExtensions
{
    public static GraphView ToGraphView(this PsBidirectionalGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var nodes = graph.Vertices
            .Select(vertex => new GraphViewNode(
                vertex.Label,
                vertex.Label,
                vertex.OriginalObject?.GetType().ToString(),
                new Dictionary<string, object?>(vertex.Metadata, StringComparer.Ordinal)))
            .ToList();

        var edges = graph.Edges
            .Select(edge => new GraphViewEdge(
                edge.Source.Label,
                edge.Target.Label,
                string.IsNullOrWhiteSpace(edge.Label) ? null : edge.Label,
                edge.Weight))
            .ToList();

        return new GraphView(nodes, edges);
    }
}