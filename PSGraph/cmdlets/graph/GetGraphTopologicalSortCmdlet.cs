using System;
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

    protected override void ProcessRecord()
    {
        if (!Graph.IsDag)
        {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("Topological sort requires a directed acyclic graph."),
                "GraphContainsCycle",
                ErrorCategory.InvalidData,
                Graph));
            return;
        }

        var direction = Reverse.IsPresent
            ? TopologicalSortDirection.Backward
            : TopologicalSortDirection.Forward;

        var sortedVertices = Graph.SourceFirstBidirectionalTopologicalSort(direction);
        WriteObject(sortedVertices, enumerateCollection: true);
    }
}
