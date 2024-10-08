﻿using System.Text;
using System.Management.Automation;
using QuikGraph;
using PSGraph.Model;
using System.Xml;
using QuikGraph.Serialization;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Import, "Graph")]
    public class ImportGraphCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Path;
        protected override void ProcessRecord()
        {
            var graph = new PsBidirectionalGraph(false);
            using (var xmlReader = XmlReader.Create(Path))
            {
                graph.DeserializeFromGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(
                    xmlReader,
                    id => new PSVertex(id),
                    (source, target, id) => new PSEdge(source, target, new PSEdgeTag()));
            }

            WriteObject(graph);
            //throw NotImplementedException(); 
        }
    }
}
