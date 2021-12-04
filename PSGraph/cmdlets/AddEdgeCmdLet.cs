using System;
using System.Management.Automation;
using QuikGraph;
using System.Globalization;

//add-edge -from $nodeFrom -to $nodeTo -attributes $attr -graph $g

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Add, "Edge")]
    public class AddEdgeCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public object From { get; set; }

        [Parameter(Mandatory = true)]
        public object To { get; set; }

        [Parameter(Mandatory = true)]
        public object Graph { get; set; }

        [Parameter]
        public object Attribute { get; set; }

        protected override void ProcessRecord()
        {

            ProcesRecordDefault();
        }

        void ProcesRecordDefault()
        {
            dynamic graph = Graph;

            if (graph is PSObject)
            {
                graph = ((PSObject)graph).ImmediateBaseObject;
            }

            if (graph == null)
            {
                throw new ArgumentException($"'Graph' mustn't be equal to null");
            }

            object from = From;
            if (from is PSObject)
            {
                from = ((PSObject)from).ImmediateBaseObject;
            }

            if (from == null)
            {
                throw new ArgumentException("'From' mustn't be equal to null");
            }

            object to = To;
            if (to is PSObject)
            {
                to = ((PSObject)to).ImmediateBaseObject;
            }

            if (to == null)
            {
                throw new ArgumentException("'To' mustn't be equal to null");
            }

            WriteVerbose("Add-Edge: Graph type is: " + Graph.GetType());
            WriteVerbose("Add-Edge: From type is: " + From.GetType());
            WriteVerbose("Add-Edge: To type is: " + To.GetType());

            object attribute = Attribute;
            if (attribute is PSObject)
            {
                attribute = ((PSObject)attribute).ImmediateBaseObject;
            }

            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type edgeType = graphGenericArgs[1];
            dynamic edge = Activator.CreateInstance(edgeType, from, to, attribute);

            bool result = graph.AddVerticesAndEdge(edge);

            WriteObject(result);
           
        }
    }
}
