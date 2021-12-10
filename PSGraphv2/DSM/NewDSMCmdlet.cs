using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.cmdlets
{
    [Cmdlet(VerbsCommon.New, "DSM")]
    public  class NewDSMCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public BidirectionalGraph<object, STaggedEdge<object, object>> Graph;
        protected override void ProcessRecord()
        {

            var d = new DesignStructureMatrix.Dsm(Graph);
            WriteObject(d);
            //base.ProcessRecord();
        }
    }
}
