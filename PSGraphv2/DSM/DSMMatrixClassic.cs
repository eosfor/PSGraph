using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.DesignStructureMatrix
{
    public class DSMMatrixClassic : DSMMatrixBase<DSMMatrixClassic>, IDSMPartitioningAlgorithm<DSMMatrixClassic>
    {


        public DSMMatrixClassic Partition()
        {
            // this works as follows:
            // 1. copy of the current DSM
            // 2. detect and remove empty lines (rows, and columns). such rows will be moved up in the matrix, and columns will go right
            // 3. identify loops by using using powers of a matrix
            // 4. combine vertices from detected loops and empty lines into a single index string
            // 5. reapply current graph to the new layout based on the index string

            var workingDsm = new DSMMatrixClassic(this);

            var indRowIdx = workingDsm.FindIndependentLines(DSMMatrixLineKind.ROW);
            var indColumnIdx = workingDsm.FindIndependentLines(DSMMatrixLineKind.COLUMN);
            var loops = ExtractLoops(workingDsm);

            Dictionary<PSVertex, int> index = MakeIndexVector(indRowIdx, indColumnIdx, loops);

            var graph = this.GraphFromDSM();
            var newDsm = new DSMMatrixClassic(graph, index, index);

            return newDsm;
        }

        private Dictionary<PSVertex, int> MakeIndexVector(List<PSVertex> indRowIdx, List<PSVertex> indColumnIdx, List<List<PSVertex>> loops)
        {
            var vertexList = indRowIdx.Concat(loops[0]);
            for (int i = 1; i < loops.Count; i++)
            {
                vertexList = vertexList.Concat(loops[i]);
            }
            vertexList = indRowIdx.Concat(vertexList);
            vertexList = vertexList.Concat(indColumnIdx);

            var distinct = vertexList.Distinct();

            var index = new Dictionary<PSVertex, int>();
            var n = 0;
            foreach (var key in distinct)
            {
                index.Add(key, n++);
            }

            return index;
        }

        private List<List<PSVertex>> ExtractLoops(DSMMatrixClassic workingDsm, int power = 2)
        {
            var loops = new List<List<PSVertex>>(); //count of the nested list shows the actual exponent
            for (int i = power; i <= workingDsm._dsm.RowCount; i++)
            {
                var currentDsm = workingDsm.Power(i);
                var loop = DetectLoops(currentDsm);
                loops.Add(loop);
            }

            loops = loops.OrderByDescending(x => x.Count).ToList();
            return loops;
        }

        private List<PSVertex> DetectLoops(DSMMatrixBase<DSMMatrixClassic> currentDsm)
        {
            //TODO: assuming there is only one loop for a given power n
            var ret = new List<PSVertex>();
            for (int currentIdx = 0; currentIdx < currentDsm.Dsm.RowCount; currentIdx++)
            {
                if (currentDsm.Dsm[currentIdx, currentIdx] > 0)
                {
                    var r = currentDsm.RowIndex.Where(x => x.Value == currentIdx).First();
                    ret.Add(r.Key);
                }
            }

            return ret;
        }


        private List<PSVertex> FindIndependentLines(DSMMatrixLineKind kind)
        {
            var sums = kind == DSMMatrixLineKind.COLUMN ? this._dsm.ColumnSums() : this._dsm.RowSums(); //assuming no negative elements

            List<int> toBeDeleted = new List<int>();
            var ret = new List<PSVertex>();
            for (int currentIdx = 0; currentIdx < this._dsm.RowCount; currentIdx++)
            {
                if (sums[currentIdx] == 0)
                {
                    toBeDeleted.Add(currentIdx);
                }
            }

            foreach (int idx in toBeDeleted)
            {
                var r = _rowIndex.Where(x => x.Value == idx).First();
                var c = _colIndex.Where(x => x.Value == idx).First();
                ret.Add(r.Key);
                _rowIndex.Remove(r.Key);
                _colIndex.Remove(c.Key);
                this._dsm = this._dsm.RemoveRow(idx);
                this._dsm = this._dsm.RemoveColumn(idx);

                foreach (var el in _rowIndex.Keys)
                {
                    if (_rowIndex[el] > r.Value) _rowIndex[el] -= 1;
                }

                foreach (var el in _colIndex.Keys)
                {
                    if (_colIndex[el] > r.Value) _colIndex[el] -= 1;
                }
            }

            return ret;
        }

        #region constructors
        public DSMMatrixClassic(PSBidirectionalGraph graph) : base(graph)
        {
        }

        public DSMMatrixClassic(DSMMatrixClassic dsm) : base(dsm)
        {
        }

        public DSMMatrixClassic(int rows, int cols) : base(rows, cols)
        {
        }

        public DSMMatrixClassic(int rows, int cols, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex) : base(rows, cols, rowIndex, colIndex)
        {
        }

        public DSMMatrixClassic(PSBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex) : base(graph, rowIndex, colIndex)
        {
        }

        #endregion constructors


    }
}