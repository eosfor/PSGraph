using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Data.Text;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Export, "DSM")]
    public class ExportDSMCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public IDsmMatrix Dsm;

        [Parameter(Mandatory = true)]
        public string Path;

        [Parameter(Mandatory = true)]
        public DSMExportTypes Format;
        protected override void ProcessRecord()
        {
            switch (Format)
            {
                case DSMExportTypes.SVG:
                    ExportSVG();
                    break;
                case DSMExportTypes.TEXT:
                    ExportText();
                    break;
                default:
                    break;
            }
        }

        private void ExportText()
        {
            //Dsm.Cluster();
            //var r = Dsm.Order();
            DelimitedWriter.Write(Path, Dsm.Dsm , ",");
        }

        private void ExportSVG()
        {
            //Dsm.Cluster();
            //var r = Dsm.Order();
            var svgDoc = ((IDsmMatrixView)Dsm).ToSvg();
            ((IDsmMatrixView)Dsm).ExportSvg(svgDoc, Path);
        }
    }
}
