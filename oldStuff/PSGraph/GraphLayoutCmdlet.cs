using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.Show, "GraphLayout")]
    class GraphLayoutCmdlet
    {
        [Parameter(Mandatory = true)]
        public object Graph { get; set; }
    }
}