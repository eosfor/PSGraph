using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using QuickGraph;

//add-edge -from $nodeFrom -to $nodeTo -attributes $attr -graph $g

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Add, "Edge")]
    public class AddEdgeCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Object From { get; set; }

        [Parameter(Mandatory = true)]
        public Object To { get; set; }

        [Parameter(Mandatory = true)]
        public Object Graph { get; set; }

        [Parameter]
        public Object Attribute { get; set; }

        protected override void ProcessRecord()
        {

            ProcesRecordDefault();
        }

        void ProcesRecordDefault()
        {
            object graph = Graph;
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

            WriteVerbose("Add-Edge: Graph type is: " + Graph.GetType().ToString());
            WriteVerbose("Add-Edge: From type is: " + From.GetType().ToString());
            WriteVerbose("Add-Edge: To type is: " + To.GetType().ToString());

            MethodInfo mi = graph.GetType().GetMethod("AddVerticesAndEdge");
            if (mi == null)
            {
                throw new System.ArgumentException("'Graph' is an object of an unknown type");
            }

            Object attribute = Attribute;
            if (attribute is PSObject)
                attribute = ((PSObject)attribute).ImmediateBaseObject;

            Type[] elType = graph.GetType().GetGenericArguments();
            Type edgeType = typeof(STaggedEdge<,>).MakeGenericType(new Type[] { elType[0], typeof(object) });
            var edge = Activator.CreateInstance(edgeType, new object[] { from, to, Attribute});

            Object result = mi.Invoke(graph, new object[] { edge });
            WriteObject(result);
        }
    }
}
