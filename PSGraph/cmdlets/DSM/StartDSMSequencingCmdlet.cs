using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    public enum LoopDetectionMethod
    {
        Powers,
        Condensation
    }
    
    [Cmdlet(VerbsLifecycle.Start, "DSMSequencing")]
    public class StartDSMSequencingCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public IDsm Dsm;

        [Parameter(Mandatory = false)]
        public LoopDetectionMethod LoopDetectionMethod = LoopDetectionMethod.Condensation;

        protected override void ProcessRecord()
        {
            IDsmLoopDetectionAlgorithm algorithm = LoopDetectionMethod switch
            {
                LoopDetectionMethod.Powers => new DsmPowersLoopDetectionAlgorithm(Dsm),
                LoopDetectionMethod.Condensation => new GraphCondensationAlgorithm(),
                _ => throw new NotImplementedException()
            };

            var algo = new DsmSequencingAlgorithm(Dsm);
            var resultDsm = algo.Sequence(algorithm);
            WriteObject(resultDsm);
        }
    }
}
