using QuikGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell;
using QuikGraph.Algorithms;
using PSGraph.Model;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Get, "GraphPath")]
    public class GetGraphPath : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSVertex From;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSVertex To;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSBidirectionalGraph Graph;

        protected override void ProcessRecord()
        {

            var tryFunc = Graph.ShortestPathsDijkstra(e => e.Weight, From);
            
            IEnumerable<PSEdge>? result = null;
            tryFunc.Invoke(To, out result);

            WriteObject(result);
        }
    }
}
