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
    public partial class DSMMatrix
    {
        private Matrix<Single> _dsm;

        Dictionary<PSVertex, int> _rowIndex = new Dictionary<PSVertex, int>();
        Dictionary<PSVertex, int> _colIndex = new Dictionary<PSVertex, int>();


        public Single this[PSVertex from, PSVertex to] { get => GetElementByVertex(from, to); set => SetElementByVertex(from, to, value); }
        public Single this[int row, int column] { get => _dsm[row, column]; set => _dsm[row, column] = value; }

        public DSMMatrix Partition()
        {
            var workingDsm = new DSMMatrix(this);

            var indRowIdx = workingDsm.FindIndependentRows();
            var indColumnIdx = workingDsm.FindIndependentColumns();

            var loops = new List<List<PSVertex>>(); //count of the nested list shows the actual exponent
            ExtractLoops(workingDsm, loops);
            loops = loops.OrderByDescending( x => x.Count).ToList();

            var vertexList = indRowIdx.Concat(loops[0]);
            for (int i = 1; i < loops.Count; i++){
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

            var newDsm = new DSMMatrix(this._dsm.RowCount, this._dsm.ColumnCount, index, index);

            var graph = this.GraphFromDSM();

            foreach (var e in graph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                var colIndex = newDsm._colIndex[to];//_rowIndex[from];
                var rowIndex = newDsm._rowIndex[from];//_colIndex[to];
                newDsm[rowIndex, colIndex] = linkWeight;
            }

            return newDsm;
        }

        private static void ExtractLoops(DSMMatrix workingDsm, List<List<PSVertex>> loops, int power = 2)
        {
            for (int i = power; i <= workingDsm._dsm.RowCount; i++)
            {
                var currentDsm = workingDsm.Power(i);
                var loop = currentDsm.DetectLoops();
                loops.Add(loop);
            }
        }

        private List<PSVertex> FindIndependentRows()
        {
            var retDsm = new DSMMatrix(this);

            var rowSums = this._dsm.RowSums(); //assuming no negative elements

            // int replaceIdx = 0;

            // for (int currentIdx = 1; currentIdx < retDsm._dsm.RowCount; currentIdx ++){
            //     if (rowSums[currentIdx] == 0) {
            //         // c -> r
            //         var c = retDsm._dsm.Row(currentIdx);
            //         var r = retDsm._dsm.Row(replaceIdx);
            //         retDsm._dsm.SetRow(replaceIdx, c);
            //         retDsm._dsm.SetRow(currentIdx, r);
            //         replaceIdx ++;
            //     }
            // }

            List<int> toBeDeleted = new List<int>();
            var ret = new List<PSVertex>();
            for (int currentIdx = 0; currentIdx < this._dsm.RowCount; currentIdx++)
            {
                if (rowSums[currentIdx] == 0)
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

        private List<PSVertex> FindIndependentColumns()
        {
            var retDsm = new DSMMatrix(this);

            var colSums = this._dsm.ColumnSums(); //assuming no negative elements

            // int replaceIdx = 0;

            // for (int currentIdx = 1; currentIdx < retDsm._dsm.ColumnCount; currentIdx ++){
            //     if (colSums[currentIdx] == 0) {
            //         // c -> r
            //         var c = retDsm._dsm.Column(currentIdx);
            //         var r = retDsm._dsm.Column(replaceIdx);
            //         retDsm._dsm.SetColumn(replaceIdx, c);
            //         retDsm._dsm.SetColumn(currentIdx, r);
            //         replaceIdx ++;
            //     }
            // }

            List<int> toBeDeleted = new List<int>();
            var ret = new List<PSVertex>();
            for (int currentIdx = 0; currentIdx < this._dsm.RowCount; currentIdx++)
            {
                if (colSums[currentIdx] == 0)
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

        private DSMMatrix Power(int exponent)
        {
            var retDsm = new DSMMatrix(this);
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
        public DSMMatrix(PSBidirectionalGraph graph)
        {
            _dsm = GraphToDSM(graph);
        }

        public DSMMatrix(DSMMatrix dsm)
        {
            this._dsm = Matrix<Single>.Build.Dense(dsm._dsm.RowCount, dsm._dsm.ColumnCount);
            dsm._dsm.CopyTo(this._dsm);
            _rowIndex = new Dictionary<PSVertex, int>(dsm._rowIndex);
            _colIndex = new Dictionary<PSVertex, int>(dsm._colIndex);
        }

        public DSMMatrix(int rows, int cols){
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = new Dictionary<PSVertex, int>();
            _colIndex = new Dictionary<PSVertex, int>();
        }

        public DSMMatrix(int rows, int cols, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex){
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }

        #endregion constructors

    }
}