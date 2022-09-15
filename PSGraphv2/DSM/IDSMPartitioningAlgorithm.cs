using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PSGraph.DesignStructureMatrix{
    public interface IDSMPartitioningAlgorithm{
        public IDsmMatrix Partition();

    }
}