using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using QuikGraph;
using DotParser;

namespace PSGraph
{
    [Cmdlet(VerbsData.Import, "Graph")]
    public class ImportGraphCmdlet: PSCmdlet
    {
        public string Path;
        protected override void ProcessRecord()
        {
            var p = DotParser.DotParser.parse(Path);

            base.ProcessRecord();
        }
    }
}
