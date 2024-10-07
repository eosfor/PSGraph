using System.Management.Automation;
using QuikGraph.Algorithms;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GraphTopologicSort")]
    public class GetGraphTopologicSort : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

        protected override void EndProcessing()
        {
            var res = Graph.TopologicalSort();
            if (res != null)
                WriteObject(res);
        }
    }
}
