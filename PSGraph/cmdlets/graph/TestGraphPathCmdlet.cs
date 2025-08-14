using System;
using System.Collections.Generic;
using System.Management.Automation;
using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "GraphPath")]
    [OutputType(typeof(bool))]
    public class TestGraphPathCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PSVertex From { get; set; } = null!;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PSVertex To { get; set; } = null!;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PsBidirectionalGraph Graph { get; set; } = null!;

        protected override void ProcessRecord()
        {
            if (From == null || To == null || Graph == null)
            {
                WriteObject(false);
                return;
            }

            // Equality for PSVertex is label-based (overrides Equals/GetHashCode),
            // so ContainsVertex already works even if caller passed a different PSVertex
            // instance with the same Label. No need for manual label resolution.
            if (From.Equals(To))
            {
                WriteObject(true);
                return;
            }

            if (!Graph.ContainsVertex(From) || !Graph.ContainsVertex(To))
            {
                WriteObject(false);
                return;
            }

            var sccAlg = new StronglyConnectedComponentsAlgorithm<PSVertex, PSEdge>(Graph);
            sccAlg.Compute();

            var compMap = sccAlg.Components;
            if (!compMap.TryGetValue(From, out var compFrom) || !compMap.TryGetValue(To, out var compTo))
            {
                WriteObject(false);
                return;
            }

            if (compFrom == compTo)
            {
                WriteObject(true);
                return;
            }

            var adj = new Dictionary<int, HashSet<int>>();
            foreach (var edge in Graph.Edges)
            {
                if (!compMap.TryGetValue(edge.Source, out var cS)) continue;
                if (!compMap.TryGetValue(edge.Target, out var cT)) continue;
                if (cS == cT) continue;
                if (!adj.TryGetValue(cS, out var set))
                {
                    set = new HashSet<int>();
                    adj[cS] = set;
                }
                set.Add(cT);
            }

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(compFrom);
            visited.Add(compFrom);
            var found = false;
            while (queue.Count > 0 && !found)
            {
                var c = queue.Dequeue();
                if (!adj.TryGetValue(c, out var outs)) continue;
                foreach (var nxt in outs)
                {
                    if (nxt == compTo)
                    {
                        found = true;
                        break;
                    }
                    if (visited.Add(nxt)) queue.Enqueue(nxt);
                }
            }

            WriteObject(found);
        }
    }
}
