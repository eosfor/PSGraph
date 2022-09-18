using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PSGraph.DesignStructureMatrix;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Start, "DSMClustering")]
    public class StartDSMClusteringCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public  IDsmMatrix Dsm;

        protected override void ProcessRecord()
        {
            ((IDsmPartitioningAlgorithm)Dsm).Partition();
            WriteObject(Dsm);
        }
    }
}
