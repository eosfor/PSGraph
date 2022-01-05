using PSGraph.Model;
using System;
using System.Management.Automation;
using System.Reflection;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public object Vertex;

        [Parameter(Mandatory = true)]
        public PSBidirectionalGraph Graph;

        protected override void ProcessRecord()
        {
            var result = Graph.AddVertex(new PSVertex(Vertex.ToString(), Vertex));
            WriteVerbose(result.ToString());
        }
    }
}
