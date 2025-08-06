using System.Collections.Generic;
using System.Management.Automation;
using QuikGraph.Algorithms.ConnectedComponents;
using PSGraph.Model;
using PSGraph.Common.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GraphConnectedComponent")]
    public class GetGraphConnectedComponent : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PsBidirectionalGraph Graph { get; set; } = null!;

        protected override void EndProcessing()
        {
            var algorithm = new StronglyConnectedComponentsAlgorithm<PSVertex, PSEdge>(Graph);
            algorithm.Compute();

            var results = new List<PSConnectedComponentRecord>();
            foreach (var pair in algorithm.Components)
            {
                results.Add(new PSConnectedComponentRecord { Vertex = pair.Key, ComponentId = pair.Value });
            }

            WriteObject(results, enumerateCollection: true);
        }
    }
}
