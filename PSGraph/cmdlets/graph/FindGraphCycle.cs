using System.Collections.Generic;
using System.Management.Automation;
using QuikGraph.Algorithms.Search;
using PSGraph.Model;
using PSGraph.Common.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet("Find", "GraphCycle")]
    public class FindGraphCycle : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PsBidirectionalGraph Graph { get; set; } = null!;

        protected override void EndProcessing()
        {
            var dfs = new DepthFirstSearchAlgorithm<PSVertex, PSEdge>(Graph);
            var parents = new Dictionary<PSVertex, PSVertex>();
            var cycles = new List<PSCycleRecord>();

            dfs.TreeEdge += e => parents[e.Target] = e.Source;
            dfs.BackEdge += e =>
            {
                var cycle = new List<PSVertex> { e.Target };
                var current = e.Source;
                while (!EqualityComparer<PSVertex>.Default.Equals(current, e.Target) && parents.TryGetValue(current, out var parent))
                {
                    cycle.Add(current);
                    current = parent;
                }
                cycle.Add(e.Target);
                cycle.Reverse();
                cycles.Add(new PSCycleRecord { Vertices = cycle });
            };

            dfs.Compute();

            WriteObject(cycles, enumerateCollection: true);
        }
    }
}
