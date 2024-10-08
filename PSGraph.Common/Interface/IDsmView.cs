using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

namespace PSGraph.DesignStructureMatrix
{
    public interface IDsmView
    {
        public abstract SvgDocument ToSvg();
        public abstract void ExportText(string Path);
        public string ExportGraphViz();
    }
}