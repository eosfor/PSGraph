using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "DSM")]
    public  class NewDSMCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public PSBidirectionalGraph Graph;
        protected override void ProcessRecord()
        {

            var d = new DSMMatrixClassic(Graph);
            WriteObject(d);
        }
    }
}
