using System.Text;
using System.Management.Automation;
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

            var g = new BidirectionalGraph<object, STaggedEdge<object, object>>();

            foreach (var node in p.Nodes)
            {
                g.AddVertex(node.Value["label"]);
            }
            

            foreach (var e in p.Edges)
            {
                var from = p.Nodes.Where(n => n.Key == e.Key.Item1).First().Value["label"];
                var to = p.Nodes.Where(n => n.Key == e.Key.Item2).First().Value["label"];
                var newEdge = new STaggedEdge<object, object>(from, to, null);
                g.AddEdge(newEdge);
            }

            WriteObject(g);

            //base.ProcessRecord();
        }
    }
}
