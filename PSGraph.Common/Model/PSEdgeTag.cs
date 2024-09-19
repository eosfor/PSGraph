using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Model
{
    public class PSEdgeTag
    {
        public string? Label;

        public PSEdgeTag()
        {
        }

        public PSEdgeTag(string? label)
        {
            Label = label;
        }
    }
}
