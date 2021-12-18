using PSGraph.Model;
using System;
using System.Management.Automation;
using System.Reflection;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public PSVertex Vertex;

        [Parameter(Mandatory = true)]
        public PSBidirectionalGraph Graph;

        protected override void ProcessRecord()
        {
            var result = Graph.AddVertex(Vertex);
            WriteVerbose(result.ToString());
        }
    }
}
