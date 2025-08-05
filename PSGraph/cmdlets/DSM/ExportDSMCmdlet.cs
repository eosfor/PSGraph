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
using PSGraph.Vega.Extensions;
using Newtonsoft.Json.Linq;
using PSGraph.Vega.Spec;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Export, "DSM", DefaultParameterSetName = "PlainDsm")]
    public class ExportDSMCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PlainDsm")]
        public IDsm Dsm;

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PartitionedDsm")]
        public PartitioningResult Result;

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "PlainDsm")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "PartitionedDsm")]
        [Parameter(Mandatory = false)]
        public string Path;

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "PlainDsm")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = "PartitionedDsm")]
        public DSMExportTypes Format = DSMExportTypes.TEXT;

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

            string result = string.Empty;

            switch (Format)
            {
                // case DSMExportTypes.SVG:
                //     result = ExportSVG();
                //     break;
                case DSMExportTypes.TEXT:
                    result = ExportText();
                    break;
                case DSMExportTypes.VEGA_JSON:
                    result = ExportVega(VegaExportTypes.JSON);
                    break;
                case DSMExportTypes.VEGA_HTML:
                    result = ExportVega(VegaExportTypes.HTML);
                    break;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                File.WriteAllText(Path, result);
            }
            else
            {
                WriteObject(result);
            }
        }

        private string ExportText()
        {
            return _view.ExportText();
        }

        private string ExportSVG()
        {
            return _view.ToSvgString();
        }

        private string ExportVega(VegaExportTypes exportType)
        {
            var modulePath = MyInvocation.MyCommand.Module?.ModuleBase;
            return _view.ToVegaSpec(exportType, modulePath);
        }
    }
}
