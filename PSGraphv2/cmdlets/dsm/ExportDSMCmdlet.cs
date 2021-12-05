using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PSGraph.DSM;
using Svg;

namespace PSGraph.cmdlets.dsm
{
    [Cmdlet(VerbsData.Export, "DSM")]
    public class ExportDSMCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public DSM.dsm Dsm;

        [Parameter(Mandatory = true)]
        public string Path;
        protected override void ProcessRecord()
        {
            var svgDoc = new SvgDocument()
            {
                Width = Dsm.Size * 10 + 200,
                Height = Dsm.Size * 10 + 200,
                ViewBox = new SvgViewBox() { MinX = 0, MinY = 0, Height = Dsm.Size * 10, Width = Dsm.Size * 10 }
            };

            var svgGroup = new SvgGroup();
            svgDoc.Children.Add(svgGroup);

            for (int i = 0; i <= Dsm.Size - 1; i++)
            {
                var row = i * 10 + 100;
                for (int j = 0; j <= Dsm.Size - 1; j++)
                {
                    var col = j * 10 + 100;
                    var rect = new SvgRectangle() { 
                        Width = 10, 
                        Height = 10, 
                        X = row, 
                        Y = col, 
                        StrokeWidth = 1, 
                        Stroke = new SvgColourServer(System.Drawing.Color.Black) 
                    };
                    if (Dsm[i, j] == 0)
                    {
                        rect.Fill = new SvgColourServer(System.Drawing.Color.White);
                    }
                    else
                    {
                        rect.Fill = new SvgColourServer(System.Drawing.Color.Red);
                    }

                    svgDoc.Children.Add(rect);

                }
            }

            svgDoc.Write(Path);

            //base.ProcessRecord();
        }
    }
}
