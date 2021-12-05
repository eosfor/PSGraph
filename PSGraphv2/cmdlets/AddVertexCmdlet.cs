using System;
using System.Management.Automation;
using System.Reflection;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public object Vertex { get; set; }

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

            MethodInfo mi = graph.GetType().GetMethod("AddVertex");
            if (mi == null)
            {
                throw new ArgumentException("'Graph' is an object of an unknown type");
            }

            bool result = (bool) mi.Invoke(graph, new[] { Vertex });

            WriteObject(result);
        }
    }
}
