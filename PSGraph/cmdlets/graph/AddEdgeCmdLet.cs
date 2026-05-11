using System;
using System.Management.Automation;
using QuikGraph;
using System.Globalization;
using PSGraph.Model;

//add-edge -from $nodeFrom -to $nodeTo -attributes $attr -graph $g

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Edge")]
    public class AddEdgeCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject From;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject To;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public IMutableVertexAndEdgeListGraph<PSVertex, PSEdge> Graph;

        [Parameter(Mandatory = false)]
        public object Tag;

        protected override void ProcessRecord()
        {
            ProcessRecordDefault();
        }

        void ProcessRecordDefault()
        {
            var newFrom = PSVertexFactory.FromPSObject(From);
            var newTo = PSVertexFactory.FromPSObject(To);

            var edge = new PSEdge(newFrom, newTo, new PSEdgeTag(Tag?.ToString()));
            var result = Graph.AddVerticesAndEdge(edge);

            WriteVerbose(result.ToString());
           
        }
    }
}
