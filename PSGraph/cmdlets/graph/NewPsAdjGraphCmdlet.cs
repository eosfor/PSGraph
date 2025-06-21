using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSGraph.Model;
using QuikGraph;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "AdjacencyGraph")]
    public class NewPsAdjGraphCmdlet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var newGraph = new PsAdjacencyGraph(false);
            WriteObject(newGraph);
        }
    }
}
