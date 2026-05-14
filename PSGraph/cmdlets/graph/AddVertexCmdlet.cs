using PSGraph.Model;
using QuikGraph;
using System;
using System.Linq;
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

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {

            var newPSVertex = PSVertexFactory.FromPSObject(Vertex);

            var graphVertex = AddOrGetGraphVertex(newPSVertex, out var result);
            WriteVerbose(result.ToString());

            if (PassThru.IsPresent)
            {
                WriteObject(graphVertex);
            }
        }

        private PSVertex AddOrGetGraphVertex(PSVertex vertex, out bool added)
        {
            if (Graph is PsBidirectionalGraph psGraph)
            {
                var before = psGraph.VertexCount;
                var graphVertex = psGraph.AddOrGetVertex(vertex);
                added = psGraph.VertexCount > before;
                return graphVertex;
            }

            added = Graph.AddVertex(vertex);
            return added
                ? vertex
                : Graph.Vertices.FirstOrDefault(v => v == vertex) ?? vertex;
        }
    }
}
