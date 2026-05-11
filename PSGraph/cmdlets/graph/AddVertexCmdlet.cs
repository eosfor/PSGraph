using PSGraph.Model;
using QuikGraph;
using System;
using System.Management.Automation;
using System.Reflection;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject Vertex;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public IMutableVertexAndEdgeListGraph<PSVertex, PSEdge> Graph;

        protected override void ProcessRecord()
        {

            var newPSVertex = PSVertexFactory.FromPSObject(Vertex);

            var result = Graph.AddVertex(newPSVertex);
            WriteVerbose(result.ToString());
        }
    }
}
