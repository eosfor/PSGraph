using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using QuickGraph;
using System.Reflection;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Object Vertex { get; set; }

        [Parameter(Mandatory = true)]
        public Object Graph { get; set; }

        [Parameter(Mandatory = false)]
        public bool Unique = true;

        protected override void ProcessRecord()
        {
            Object graph = Graph;
            if (graph is PSObject)
                graph = ((PSObject)graph).ImmediateBaseObject;
            if (graph == null)
            {
                throw new System.ArgumentException("'Graph' mustn't be equal to null");
            }

            MethodInfo mi = graph.GetType().GetMethod("AddVertex");
            if (mi == null)
            {
                throw new System.ArgumentException("'Graph' is an object of an unknown type");
            }

            mi.Invoke(graph, new object[] { Vertex });
        }

    }
}
