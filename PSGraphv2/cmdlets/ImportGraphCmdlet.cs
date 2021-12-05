using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using QuikGraph;
using DotParser;
using QuikGraph;

namespace PSGraph
{
    [Cmdlet(VerbsData.Import, "Graph")]
    public class ImportGraphCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? Path;
        protected override void ProcessRecord()
        {
            var s = File.ReadAllText(Path, Encoding.UTF8);
            var p = DotParser.DotParser.parse(s);

            var g = new BidirectionalGraph<object, Edge<object>>();

            foreach (var node in p.Nodes)
            {
                g.AddVertex(node.Value["label"]);
            }
            

            foreach (var e in p.Edges)
            {
                var from = p.Nodes.Where(n => n.Key == e.Key.Item1).First().Value["label"];
                var to = p.Nodes.Where(n => n.Key == e.Key.Item2).First().Value["label"];
                var newEdge = new Edge<object>(from, to);
                g.AddEdge(newEdge);
            }

            WriteObject(g);

            //base.ProcessRecord();
        }
    }
}
