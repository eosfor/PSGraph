using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PSGraph.DesignStructureMatrix;

namespace PSGraph.cmdlets.dsm
{
    [Cmdlet(VerbsData.Export, "DSM")]
    public class ExportDSMCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public DesignStructureMatrix.Dsm Dsm;

        [Parameter(Mandatory = true)]
        public string Path;
        protected override void ProcessRecord()
        {


            //base.ProcessRecord();
        }
    }
}
