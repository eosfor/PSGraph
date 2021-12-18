using JetBrains.Annotations;
using QuikGraph;
using QuikGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Model
{
    public class PSEdge : TaggedEdge<PSVertex, PSEdgeTag>
    {
        public string Label;
        public string Name => Label;
        public int Weight = 1;
        public GraphvizEdge GVertexParameters = new GraphvizEdge();
        public PSEdge([JetBrains.Annotations.NotNullAttribute] PSVertex source, [JetBrains.Annotations.NotNullAttribute] PSVertex target, [CanBeNullAttribute] PSEdgeTag tag) : base(source, target, tag)
        {
        }
    }
}
