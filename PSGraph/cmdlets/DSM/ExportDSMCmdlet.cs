using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Data.Text;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using QuikGraph;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Export, "DSM", DefaultParameterSetName = "PlainDsm")]
    public class ExportDSMCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PlainDsm")]
        public IDsm Dsm;
        
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PartitionedDsm")]
        public PartitioningResult Result;
        
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "PlainDsm")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "PartitionedDsm")]
        [Parameter(Mandatory = true)]
        public string Path;

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "PlainDsm")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = "PartitionedDsm")]
        public DSMExportTypes Format = DSMExportTypes.SVG;

        private IDsm _dsm;
        private IDsmPartitionAlgorithm? _algo;
        private IDsmView _view;
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                default:
                case "PlainDsm":
                    _dsm = Dsm;
                    _view = new DsmView(_dsm);
                    break;
                case "PartitionedDsm":
                    _dsm = Result.Dsm;
                    _algo = Result.Algorithm;
                    _view = new DsmView(_dsm, _algo.Partitions);
                    break;
            }


            switch (Format)
            {
                default:
                case DSMExportTypes.SVG:
                    ExportSVG();
                    break;
                case DSMExportTypes.TEXT:
                    ExportText();
                    break;
            }
        }

        private void ExportText()
        {
            _view.ExportText(Path);
        }

        private void ExportSVG()
        {
            var svgDoc = _view.ToSvg();
            svgDoc.Write(Path);
        }
    }
}
