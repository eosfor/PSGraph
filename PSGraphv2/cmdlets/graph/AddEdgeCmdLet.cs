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
        public object From;

        [Parameter(Mandatory = true)]
        public object To;

        [Parameter(Mandatory = true)]
        public PSBidirectionalGraph Graph;

        [Parameter(Mandatory = false)]
        public object Tag;

        protected override void ProcessRecord()
        {
            ProcesRecordDefault();
        }

        void ProcesRecordDefault()
        {
            var edge = new PSEdge(new PSVertex(From.ToString(), From), new PSVertex(To.ToString(), To), new PSEdgeTag(Tag?.ToString()));
            var result = Graph.AddVerticesAndEdge(edge);

            WriteVerbose(result.ToString());
           
        }
    }
}
