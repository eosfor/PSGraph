using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSGraph.Model;
using QuikGraph;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "Graph")]
    public class NewPsGraphCmdlet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var newGraph = new PsBidirectionalGraph(false);
            WriteObject(newGraph);
        }
    }
}
