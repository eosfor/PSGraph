﻿using QuikGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell;
using QuikGraph.Algorithms;
using PSGraph.Model;

namespace PSGraph.Cmdlets
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
        public PsBidirectionalGraph Graph;

        protected override void ProcessRecord()
        {

            var tryFunc = Graph.ShortestPathsDijkstra(e => e.Weight, From);
            
            IEnumerable<PSEdge>? result = null;
            tryFunc.Invoke(To, out result);

            if (null != result) 
                WriteObject(result);
        }
    }
}
