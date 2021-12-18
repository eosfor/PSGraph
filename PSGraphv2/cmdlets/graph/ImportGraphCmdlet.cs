using System.Text;
using System.Management.Automation;
using QuikGraph;
using PSGraph.Model;
using System.Xml;
using QuikGraph.Serialization;

namespace PSGraph
{
    [Cmdlet(VerbsData.Import, "Graph")]
    public class ImportGraphCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Path;
        protected override void ProcessRecord()
        {
            //var graph = new PSBidirectionalGraph(false);
            //using (var xmlReader = XmlReader.Create(Path))
            //{
            //    graph.DeserializeFromGraphML<PSVertex, PSEdge, PSBidirectionalGraph>(
            //        xmlReader,
            //        v => v,
            //        (source, target, id) => new PSEdge(source, target));
            //}

            throw new NotImplementedException();
        }
    }
}
