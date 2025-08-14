using QuikGraph;
using QuikGraph.Algorithms.Condensation;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public static class CondensationGraphExtensions
{
    public static PsBidirectionalGraph ToPsBidirectionalGraph(this IMutableBidirectionalGraph<PsBidirectionalGraph, CondensedEdge<PSVertex, PSEdge, PsBidirectionalGraph>> condensedGraph)
    {
        var resultGraph = new PsBidirectionalGraph();

        // Каждая вершина condensedGraph — это PsBidirectionalGraph (SCC)
        foreach (var scc in condensedGraph.Vertices)
        {
            // Добавляем все вершины SCC в итоговый граф
            foreach (var vertex in scc.Vertices)
            {
                if (!resultGraph.Vertices.Contains(vertex))
                    resultGraph.AddVertex(vertex);
            }
        }

        // Добавляем рёбра между SCC
        foreach (var edge in condensedGraph.Edges)
        {
            // Для каждого ребра между SCC берём любые две вершины из SCC
            var source = edge.Source.Vertices.FirstOrDefault();
            var target = edge.Target.Vertices.FirstOrDefault();
            if (source != null && target != null)
            {
                resultGraph.AddEdge(new PSEdge(source, target));
            }
        }

        return resultGraph;
    }
}