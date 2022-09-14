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
    public class DSMMatrixBase<T> where T : DSMMatrixBase<T>
    {
        protected Matrix<Single> _dsm;

        protected Dictionary<PSVertex, int> _rowIndex = new Dictionary<PSVertex, int>();
        protected Dictionary<PSVertex, int> _colIndex = new Dictionary<PSVertex, int>();

        public Dictionary<PSVertex, int> RowIndex { get => _rowIndex; }
        public Dictionary<PSVertex, int> ColIndex { get => _colIndex; }

        public Matrix<Single> Dsm { get => _dsm; }

        public Single this[PSVertex from, PSVertex to] { get => GetElementByVertex(from, to); set => SetElementByVertex(from, to, value); }
        public Single this[int row, int column] { get => _dsm[row, column]; set => _dsm[row, column] = value; }

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

                // TODO: support tag as weight?;

                var colIndex = _colIndex[to];
                var rowIndex = _rowIndex[from];
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

        public DSMMatrixBase<T> Power(int exponent)
        {
            var retDsm = new DSMMatrixBase<T>(this);
            retDsm._dsm = this._dsm.Power(exponent);

            return retDsm;
        }


        #region constructors
        public DSMMatrixBase(PSBidirectionalGraph graph)
        {
            _dsm = GraphToDSM(graph);
        }

        public DSMMatrixBase(DSMMatrixBase<T> dsm)
        {
            this._dsm = Matrix<Single>.Build.Dense(dsm._dsm.RowCount, dsm._dsm.ColumnCount);
            dsm._dsm.CopyTo(this._dsm);
            _rowIndex = new Dictionary<PSVertex, int>(dsm._rowIndex);
            _colIndex = new Dictionary<PSVertex, int>(dsm._colIndex);
        }

        public DSMMatrixBase(int rows, int cols)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = new Dictionary<PSVertex, int>();
            _colIndex = new Dictionary<PSVertex, int>();
        }

        public DSMMatrixBase(int rows, int cols, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }

        public DSMMatrixBase(PSBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
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