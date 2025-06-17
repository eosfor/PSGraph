using System.Management.Automation;
using PSGraph.Model;

namespace PSGraph.Cmdlets;

[Cmdlet(VerbsCommon.Get, "InEdge")]
public class GetInEdgeCmdlet: PSCmdlet
{
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSVertex Vertex;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

    protected override void EndProcessing()
    {
        IEnumerable<PSEdge> edges;
        Graph.TryGetInEdges(Vertex, out edges);

        if (edges.Count() > 0)
            WriteObject(edges);
    }
}

[Cmdlet(VerbsCommon.Get, "OutEdge")]
public class GetOutEdgeCmdlet: PSCmdlet
{
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSVertex Vertex;


        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

    protected override void EndProcessing()
    {
        IEnumerable<PSEdge> edges;
        Graph.TryGetOutEdges(Vertex, out edges);

        if (edges.Count() > 0)
            WriteObject(edges);
    }
}

