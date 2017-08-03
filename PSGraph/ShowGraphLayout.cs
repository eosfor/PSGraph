using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using GraphSharp.Controls;
using QuickGraph;

namespace PSGraph
{

    [Cmdlet(VerbsCommon.Show, "GraphLayout")]
    public class ShowGraphLayout : PSCmdlet
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

            GraphLayoutWindow w = new GraphLayoutWindow(graph);
            w.ShowDialog();
            
            //base.ProcessRecord();
        }
    }
}
