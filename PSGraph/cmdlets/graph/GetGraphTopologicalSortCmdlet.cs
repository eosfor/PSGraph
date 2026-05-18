using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSGraph.Model;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;

namespace PSGraph.Cmdlets;

[Cmdlet(VerbsCommon.Get, "GraphTopologicalSort")]
public class GetGraphTopologicalSortCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public PsBidirectionalGraph Graph { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public SwitchParameter Reverse { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateNotNull]
    public PSVertex? StartVertex { get; set; }

    protected override void ProcessRecord()
    {
        var graphToSort = StartVertex is null
            ? Graph
            : CreateReachableSubgraph(StartVertex);

        if (!graphToSort.IsDag)
        {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("Topological sort requires a directed acyclic graph."),
                "GraphContainsCycle",
                ErrorCategory.InvalidData,
                graphToSort));
            return;
        }

        var direction = Reverse.IsPresent
            ? TopologicalSortDirection.Backward
            : TopologicalSortDirection.Forward;

        var sortedVertices = graphToSort.SourceFirstBidirectionalTopologicalSort(direction);
        WriteObject(sortedVertices, enumerateCollection: true);
    }

    private PsBidirectionalGraph CreateReachableSubgraph(PSVertex startVertex)
    {
        var canonicalStartVertex = Graph.Vertices.FirstOrDefault(v => v.Equals(startVertex));
        if (canonicalStartVertex is null)
        {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("The graph does not contain the provided start vertex."),
                "StartVertexNotFound",
                ErrorCategory.ObjectNotFound,
                startVertex));
            return new PsBidirectionalGraph(Graph.AllowParallelEdges, Graph.UseNonUniqueLabels);
        }

        var reachableVertices = GetReachableVertices(canonicalStartVertex);
        var subgraph = new PsBidirectionalGraph(Graph.AllowParallelEdges, Graph.UseNonUniqueLabels);
        subgraph.AddVertexRange(reachableVertices);
        subgraph.AddEdgeRange(Graph.Edges.Where(edge =>
            reachableVertices.Contains(edge.Source) &&
            reachableVertices.Contains(edge.Target)));

        return subgraph;
    }

    private HashSet<PSVertex> GetReachableVertices(PSVertex startVertex)
    {
        var reachableVertices = new HashSet<PSVertex>();
        var pendingVertices = new Stack<PSVertex>();
        pendingVertices.Push(startVertex);

        while (pendingVertices.Count > 0)
        {
            var currentVertex = pendingVertices.Pop();
            if (!reachableVertices.Add(currentVertex))
            {
                continue;
            }

            if (!Graph.TryGetOutEdges(currentVertex, out var outEdges))
            {
                continue;
            }

            foreach (var edge in outEdges)
            {
                pendingVertices.Push(edge.Target);
            }
        }

        return reachableVertices;
    }
}
