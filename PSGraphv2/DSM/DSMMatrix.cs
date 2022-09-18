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
    public partial class DsmMatrix
    {
        private Matrix<Single> _dsm;

        protected Dictionary<PSVertex, int> _rowIndex = new Dictionary<PSVertex, int>();
        protected Dictionary<PSVertex, int> _colIndex = new Dictionary<PSVertex, int>();

        public Dictionary<PSVertex, int> RowIndex { get => _rowIndex; }
        public Dictionary<PSVertex, int> ColIndex { get => _colIndex; }

        public Matrix<Single> Dsm { get => _dsm; }


        public Single this[PSVertex from, PSVertex to] { get => GetElementByVertex(from, to); set => SetElementByVertex(from, to, value); }
        public Single this[int row, int column] { get => _dsm[row, column]; set => _dsm[row, column] = value; }

        public DsmMatrix Partition()
        {
            // this works as follows:
            // 1. copy of the current DSM
            // 2. detect and remove empty lines (rows, and columns). such rows will be moved up in the matrix, and columns will go right
            // 3. identify loops by using using powers of a matrix
            // 4. combine vertices from detected loops and empty lines into a single index string
            // 5. reapply current graph to the new layout based on the index string

            var workingDsm = new DsmMatrix(this);

            var indRowIdx = workingDsm.FindIndependentLines(DSMMatrixLineKind.ROW);
            var indColumnIdx = workingDsm.FindIndependentLines(DSMMatrixLineKind.COLUMN);
            var loops = ExtractLoops(workingDsm);

            Dictionary<PSVertex, int> index = MakeIndexVector(indRowIdx, indColumnIdx, loops);

            var graph = this.GraphFromDSM();
            var newDsm = new DsmMatrix(graph, index, index);

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

        private List<List<PSVertex>> ExtractLoops(DsmMatrix workingDsm, int power = 2)
        {
            var loops = new List<List<PSVertex>>(); //count of the nested list shows the actual exponent
            for (int i = power; i <= workingDsm._dsm.RowCount; i++)
            {
                var currentDsm = workingDsm.Power(i);
                var loop = currentDsm.DetectLoops();
                loops.Add(loop);
            }

            loops = loops.OrderByDescending(x => x.Count).ToList();
            return loops;
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

        private DsmMatrix Power(int exponent)
        {
            var retDsm = new DsmMatrix(this);
            retDsm._dsm = this._dsm.Power(exponent);

            return retDsm;
        }

        private List<PSVertex> DetectLoops()
        {
            //TODO: assuming there is only one loop for a given power n
            var ret = new List<PSVertex>();
            for (int currentIdx = 0; currentIdx < this._dsm.RowCount; currentIdx++)
            {
                if (this._dsm[currentIdx, currentIdx] > 0)
                {
                    var r = _rowIndex.Where(x => x.Value == currentIdx).First();
                    ret.Add(r.Key);
                }
            }

            return ret;
        }

        private void SetElementByVertex(PSVertex row, PSVertex column, float value)
        {
            _dsm[_rowIndex[row], _colIndex[column]] = value;
        }

        private float GetElementByVertex(PSVertex from, PSVertex to)
        {
            return _dsm[_rowIndex[from], _colIndex[to]];
        }

        private Matrix<float>? GraphToDSM(PSBidirectionalGraph graph)
        {
            Matrix<Single> dsm = Matrix<Single>.Build.Dense(graph.VertexCount, graph.VertexCount);

            int idx = 0;
            foreach (var item in graph.Vertices)
            {
                _colIndex[item] = idx;
                _rowIndex[item] = idx;
                idx++;
            }

            foreach (var e in graph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                // TODO: support tag as weight;

                var colIndex = _colIndex[to];//_rowIndex[from];
                var rowIndex = _rowIndex[from];//_colIndex[to];
                dsm[rowIndex, colIndex] = linkWeight;
            }

            return dsm;
        }

        public PSBidirectionalGraph GraphFromDSM()
        {
            var ret = new PSBidirectionalGraph();

            foreach (var row in _rowIndex)
            {
                foreach (var col in _colIndex)
                {
                    if (this[row.Key, col.Key] > 0)
                    {
                        ret.AddVerticesAndEdge(new PSEdge(row.Key, col.Key, new PSEdgeTag()));
                    }
                }
            }

            return ret;

        }

        #region constructors
        public DsmMatrix(PSBidirectionalGraph graph)
        {
            _dsm = GraphToDSM(graph);
        }

        public DsmMatrix(DsmMatrix dsm)
        {
            this._dsm = Matrix<Single>.Build.Dense(dsm._dsm.RowCount, dsm._dsm.ColumnCount);
            dsm._dsm.CopyTo(this._dsm);
            _rowIndex = new Dictionary<PSVertex, int>(dsm._rowIndex);
            _colIndex = new Dictionary<PSVertex, int>(dsm._colIndex);
        }

        public DsmMatrix(int rows, int cols)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = new Dictionary<PSVertex, int>();
            _colIndex = new Dictionary<PSVertex, int>();
        }

        public DsmMatrix(int rows, int cols, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }

        public DsmMatrix(PSBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
        {
            _rowIndex = rowIndex;
            _colIndex = colIndex;

            _dsm = Matrix<Single>.Build.Dense(graph.VertexCount, graph.VertexCount);
            
            foreach (var e in graph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                this[_rowIndex[from], _colIndex[to]] = linkWeight;
            }
        }

        #endregion constructors

    }
}