using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSGraph.Common.Model;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GraphCentrality")]
    public class GetGraphCentrality : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public PsBidirectionalGraph Graph { get; set; } = null!;

        protected override void EndProcessing()
        {
            var centrality = ComputeBetweennessCentrality(Graph);
            var results = new List<PSCentralityRecord>();
            foreach (var kvp in centrality)
            {
                results.Add(new PSCentralityRecord { Vertex = kvp.Key, Centrality = kvp.Value });
            }
            WriteObject(results, enumerateCollection: true);
        }

        private static Dictionary<PSVertex, double> ComputeBetweennessCentrality(PsBidirectionalGraph graph)
        {
            var Cb = graph.Vertices.ToDictionary(v => v, v => 0.0);
            foreach (var s in graph.Vertices)
            {
                var S = new Stack<PSVertex>();
                var P = new Dictionary<PSVertex, List<PSVertex>>();
                var sigma = new Dictionary<PSVertex, double>();
                var dist = new Dictionary<PSVertex, int>();

                foreach (var v in graph.Vertices)
                {
                    P[v] = new List<PSVertex>();
                    sigma[v] = 0.0;
                    dist[v] = -1;
                }
                sigma[s] = 1.0;
                dist[s] = 0;
                var Q = new Queue<PSVertex>();
                Q.Enqueue(s);
                while (Q.Count > 0)
                {
                    var v = Q.Dequeue();
                    S.Push(v);
                    foreach (var edge in graph.OutEdges(v))
                    {
                        var w = edge.Target;
                        if (dist[w] < 0)
                        {
                            Q.Enqueue(w);
                            dist[w] = dist[v] + 1;
                        }
                        if (dist[w] == dist[v] + 1)
                        {
                            sigma[w] += sigma[v];
                            P[w].Add(v);
                        }
                    }
                }
                var delta = graph.Vertices.ToDictionary(v => v, v => 0.0);
                while (S.Count > 0)
                {
                    var w = S.Pop();
                    foreach (var v in P[w])
                    {
                        delta[v] += (sigma[v] / sigma[w]) * (1.0 + delta[w]);
                    }
                    if (!EqualityComparer<PSVertex>.Default.Equals(w, s))
                        Cb[w] += delta[w];
                }
            }
            return Cb;
        }
    }
}
