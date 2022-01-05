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
    public class DsmStorage
    {
        private Matrix<Single> _dsm;

        private Dictionary<PSVertex, int> _rowIndex = new Dictionary<PSVertex, int>();
        private Dictionary<PSVertex, int> _colIndex = new Dictionary<PSVertex, int>();
        private PSBidirectionalGraph _sourceGraph;

        public Single this[PSVertex from, PSVertex to] { get => FindDsmElementByGraphVertex(from, to); set => SetDsmElementByGraphVertex(from, to, value);  }

        public List<PSVertex> Objects { get => GetDsmElementNames(); }
        public int Size => this.Objects.Count;
        public int Count => (this._dsm.RowCount * this._dsm.ColumnCount);

        public Matrix<float> Dsm { get => _dsm; }

        public Dictionary<PSVertex, int> RowIndex => _rowIndex;
        public Dictionary<PSVertex, int> ColIndex => _colIndex;


        public Single GetByIndex(int i, int j)
        {
            return _dsm[i, j];
        }

        public float[] Row(PSVertex obj)
        {
            var idx = _rowIndex[obj];
            return _dsm.Row(idx).ToArray();
        }

        public float[] Column(PSVertex obj)
        {
            var idx = _colIndex[obj];
            return _dsm.Column(idx).ToArray();
        }


        private void SetDsmElementByGraphVertex(PSVertex from, PSVertex to, float value)
        {
            var colIndex = _colIndex[from];
            var rowIndex = _rowIndex[to];

            _dsm[rowIndex, colIndex] = value;

            //throw new NotImplementedException();
        }

        public DsmStorage GetOrderedDsm(List<DSMCluster> clusters) 
        {
            Dictionary<PSVertex, int> rowIndex = new Dictionary<PSVertex, int>();
            Dictionary<PSVertex, int> colIndex = new Dictionary<PSVertex, int>();
            List<PSVertex> index = new List<PSVertex>();
            int i = 0;
            foreach (var cluster in clusters)
            {
                foreach (var item in cluster.Objects)
                {
                    index.Add(item);
                    rowIndex[item] = i;
                    colIndex[item] = i;
                    i++;
                }
            }

            Matrix<Single> ret = Matrix<Single>.Build.Dense(_sourceGraph.VertexCount, _sourceGraph.VertexCount);
            _dsm.CopyTo(ret);

            var newDsmStorage = new DsmStorage(ret, rowIndex, colIndex, _sourceGraph);
            foreach (var item1 in index)
            {
                foreach (var item2 in index)
                {
                    newDsmStorage[item1, item2] = this[item1, item2];
                }
            }

            return newDsmStorage;
        }

        public void SwapRowsByObject(PSVertex source, PSVertex target)
        {
            var sourceIdx = _rowIndex[source];
            var targetIdx = _rowIndex[target];

            SwapRows(sourceIdx, targetIdx);
        }

        public void SwapColumnsByObject(PSVertex source, PSVertex target)
        {
            var sourceIdx = _rowIndex[source];
            var targetIdx = _rowIndex[target];

            SwapColumns(sourceIdx, targetIdx);
        }
        private void SwapRows(int sourceIdx, int targetIdx)
        {
            // swap data rows
            var temp = _dsm.Row(targetIdx);
            _dsm.SetRow(targetIdx, _dsm.Row(sourceIdx));
            _dsm.SetRow(sourceIdx, temp);

            // swap index records
            var sourceDictionaryRecord = _rowIndex.Where(e => e.Value == sourceIdx).First();
            var targetDictionaryRecord = _rowIndex.Where(e => e.Value == targetIdx).First();

            var sourceKey = sourceDictionaryRecord.Key;
            var targetKey = targetDictionaryRecord.Key;

            _rowIndex.Remove(sourceDictionaryRecord.Key);
            _rowIndex.Remove(targetDictionaryRecord.Key);

            _rowIndex.Add(sourceKey, targetIdx);
            _rowIndex.Add(targetKey, sourceIdx);
        }

        private void SwapColumns(int sourceIdx, int targetIdx)
        {
            // swap data rows
            var temp = _dsm.Column(targetIdx);
            _dsm.SetColumn(targetIdx, _dsm.Column(sourceIdx));
            _dsm.SetColumn(sourceIdx, temp);

            // swap index records
            var sourceDictionaryRecord = _colIndex.Where(e => e.Value == sourceIdx).First();
            var targetDictionaryRecord = _colIndex.Where(e => e.Value == targetIdx).First();

            var sourceKey = sourceDictionaryRecord.Key;
            var targetKey = targetDictionaryRecord.Key;

            _colIndex.Remove(sourceDictionaryRecord.Key);
            _colIndex.Remove(targetDictionaryRecord.Key);

            _colIndex.Add(sourceKey, targetIdx);
            _colIndex.Add(targetKey, sourceIdx);
        }

        private List<PSVertex> GetDsmElementNames()
        {
            return _colIndex.Keys.ToList();
        }

        private float FindDsmElementByGraphVertex(PSVertex from, PSVertex to)
        {
            if (from == to)
            {
                return 0;
            }

            var colIndex = _colIndex[from];
            var rowIndex = _rowIndex[to];

            return _dsm[rowIndex, colIndex];
        }

        private Matrix<Single> GraphToDSM()
        {
            Matrix<Single> dsm = Matrix<Single>.Build.Dense(_sourceGraph.VertexCount, _sourceGraph.VertexCount);


            int idx = 0;
            foreach (var item in _sourceGraph.Vertices)
            {
                _colIndex[item] = idx;
                _rowIndex[item] = idx;
                idx++;
            }

            foreach (var e in _sourceGraph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                // TODO: support tag as weight;

                var colIndex = _rowIndex[from];
                var rowIndex = _colIndex[to];
                dsm[rowIndex, colIndex] = linkWeight;
            }

            return dsm;
        }




        #region constructors
        public DsmStorage(PSBidirectionalGraph graph)
        {
            _sourceGraph = graph;
            _dsm = GraphToDSM();
        }

        protected DsmStorage(Matrix<Single> dsm,
                             Dictionary<PSVertex, int> rowIndex,
                             Dictionary<PSVertex, int> colIndex,
                             PSBidirectionalGraph graph)
        {
            _sourceGraph = graph;
            _dsm = dsm;
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }
        #endregion constructors

    }
}

