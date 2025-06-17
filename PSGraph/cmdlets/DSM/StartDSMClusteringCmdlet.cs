using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Start, "DSMClustering")]
    public class StartDSMClusteringCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public  IDsm Dsm;
        
        [Parameter(Mandatory = false)]
        public  DsmPartitioningAlgorithms ClusteringAlgorithm = DsmPartitioningAlgorithms.Classic;

        protected override void ProcessRecord()
        {
            IDsm ret;
            IDsmPartitionAlgorithm algo;
            switch (ClusteringAlgorithm)
            {
                default:
                case DsmPartitioningAlgorithms.Classic:
                    algo = new DsmClassicPartitioningAlgorithm((DsmClassic)Dsm);
                    break;
                case DsmPartitioningAlgorithms.GraphBased:
                    algo = new DsmClassicPartitioningAlgorithm((DsmClassic)Dsm);
                    break;
                    
            }
            
            ret = algo.Partition();

            var result = new PartitioningResult() { Dsm = ret, Algorithm = algo };
            
            WriteObject(result);
        }
    }
}
