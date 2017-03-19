using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Algorithms;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Get, "GraphPath")]
    public class GetGraphPath : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Object From { get; set; }

        [Parameter(Mandatory = true)]
        public Object To { get; set; }

        [Parameter(Mandatory = true)]
        public Object Graph { get; set; }

        protected override void ProcessRecord()
        {
            Object graph = Graph;
            if (graph is PSObject)
                graph = ((PSObject)graph).ImmediateBaseObject;
            if (graph == null)
            {
                throw new System.ArgumentException("'Graph' mustn't be equal to null");
            }

            Object from = From;
            if (from is PSObject)
                from = ((PSObject)from).ImmediateBaseObject;
            if (from == null)
            {
                throw new System.ArgumentException("'From' mustn't be equal to null");
            }

            Object to = To;
            if (to is PSObject)
                to = ((PSObject)to).ImmediateBaseObject;
            if (to == null)
            {
                throw new System.ArgumentException("'To' mustn't be equal to null");
            }


            Func<STaggedEdge<object, object>, double> edgeWeights = (j => { return 1; }); //all edges are equal
            var tryGetFunc = ((AdjacencyGraph<object, STaggedEdge<object, object>>)graph).ShortestPathsDijkstra(edgeWeights, from);

            IEnumerable<STaggedEdge<object, object>> path;
            if (tryGetFunc(to, out path))
            {
                WriteObject(path);
            }
        }
    }
}