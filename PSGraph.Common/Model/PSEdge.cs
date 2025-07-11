﻿using QuikGraph;
using QuikGraph.Graphviz.Dot;

namespace PSGraph.Model
{
    public class PSEdge : TaggedEdge<PSVertex, PSEdgeTag>
    {
        public string Label;
        public string Name => Label;
        public int Weight = 1;
        public GraphvizEdge GVertexParameters = new GraphvizEdge();
        public PSEdge(PSVertex source, PSVertex target, PSEdgeTag tag) : base(source, target, tag)
        {
        }

        public static implicit operator PSUndirectedEdge(PSEdge e)
        {
            return new PSUndirectedEdge(e.Source, e.Target, 1);
        }
    }

    public class PSUndirectedEdge : IUndirectedEdge<PSVertex>, ITagged<double>
    {
        public PSVertex Source { get; }
        public PSVertex Target { get; }

        public PSUndirectedEdge(PSVertex s, PSVertex t, double tag)
        {
            var res = s.CompareTo(t);
            if (res < 0)
            {
                this.Source = s;
                this.Target = t;
            }

            if (res == 0)
            {
                this.Source = s;
                this.Target = t;
            }
            
            if (res > 0)
            {
                this.Source = t;
                this.Target = s;
            }
            
            this.Tag = tag;
        }

        public double Tag { get; set; }
        public event EventHandler? TagChanged;
    }
}
