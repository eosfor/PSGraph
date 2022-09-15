using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.DesignStructureMatrix {
    public interface IDsmMatrix {

        public Dictionary<PSVertex, int> RowIndex { get; }
        public Dictionary<PSVertex, int> ColIndex { get; }

        public Matrix<Single> Dsm { get; }

        public Single this[PSVertex from, PSVertex to] { get; set; }
        public Single this[int row, int column] { get; set; }

        public PSBidirectionalGraph GraphFromDSM();

        public IDsmMatrix Power(int exponent);
    }
}