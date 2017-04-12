using QuickGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell;
using QuickGraph.Algorithms;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Get, "GraphPath")]
    public class GetGraphPath : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public object From { get; set; }

        [Parameter(Mandatory = true)]
        public object To { get; set; }

        [Parameter(Mandatory = true)]
        public object Graph { get; set; }

        protected override void ProcessRecord()
        {
            dynamic graph = Graph;

            if (graph is PSObject)
            {
                graph = ((PSObject)graph).ImmediateBaseObject;
            }
            if (graph == null)
            {
                throw new ArgumentException("'Graph' mustn't be equal to null");
            }

            dynamic from = From;
            if (from is PSObject)
            {
                from = ((PSObject)from).ImmediateBaseObject;
            }
            if (from == null)
            {
                throw new ArgumentException("'From' mustn't be equal to null");
            }

            dynamic to = To;
            if (to is PSObject)
            {
                to = ((PSObject)to).ImmediateBaseObject;
            }
            if (to == null)
            {
                throw new ArgumentException("'To' mustn't be equal to null");
            }

            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type edgeType = graphGenericArgs[1];

            Type edgeWeightsFuncType = typeof(Func<,>).MakeGenericType(edgeType, typeof(double));
            var methodInfo = typeof(GetGraphPath).GetMethod(nameof(EqualEdgeWeights), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(edgeType);
            dynamic edgeWeightsFunc = Delegate.CreateDelegate(edgeWeightsFuncType, methodInfo);

            dynamic tryGetFunc = AlgorithmExtensions.ShortestPathsDijkstra(graph, edgeWeightsFunc, from);

            object[] tryGetFuncParams = { to, null };
            if (tryGetFunc.DynamicInvoke(tryGetFuncParams))
            {
                WriteObject(tryGetFuncParams[1]);
            }
        }

        // weight for all edges = 1
        private static double EqualEdgeWeights<TEdgeT>(TEdgeT j) => 1;
    }
}
