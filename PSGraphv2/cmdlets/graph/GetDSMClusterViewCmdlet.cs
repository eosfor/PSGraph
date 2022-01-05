using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DSMClusterView")]
    public class GetDSMClusterViewCmdlet: PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public DesignStructureMatrix.Dsm Dsm;

        protected override void ProcessRecord()
        {
            WriteObject(Dsm.GetClusteredViewGraph());
        }

    }
}
