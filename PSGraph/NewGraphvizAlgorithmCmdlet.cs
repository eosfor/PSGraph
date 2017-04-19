using QuickGraph.Graphviz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.New, "GraphvizAlgorithm")]
    public class NewGraphvizAlgorithmCmdlet:  PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public object Graph { get; set; }

        protected override void ProcessRecord()
        {
            object graph = Graph;
            if (graph is PSObject)
            {
                graph = ((PSObject)graph).ImmediateBaseObject;
            }
            if (graph == null)
            {
                throw new ArgumentException("'Graph' mustn't be equal to null");
            }

            WriteVerbose("Add-Edge: Graph type is: " + Graph.GetType().ToString());

            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];
            Type graphvizAlgType = typeof(GraphvizAlgorithm<,>).MakeGenericType(vertexType, edgeType);

            dynamic graphviz = Activator.CreateInstance(graphvizAlgType, graph);

            WriteObject(graphviz);
        }
    }
}
