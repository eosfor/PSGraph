using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Cmdlets
{
    [Cmdlet("Run", "DSMClustering")]
    public class RunDSMClusteringCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public DesignStructureMatrix.Dsm Dsm;

        protected override void ProcessRecord()
        {
            Dsm.Cluster();
            var r = Dsm.Order();
            WriteObject(r);
            //base.ProcessRecord();
        }
    }
}
