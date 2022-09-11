using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Start, "DSMClustering")]
    public class StartDSMClusteringCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public DesignStructureMatrix.Dsm Dsm;

        protected override void ProcessRecord()
        {
            Dsm.Cluster();
            Dsm.Order();
            WriteObject(Dsm);
            //base.ProcessRecord();
        }
    }
}
