using MathNet.Numerics.LinearAlgebra;
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

        private Dictionary<object, int> _rowIndex = new Dictionary<object, int>();
        private Dictionary<object, int> _colIndex = new Dictionary<object, int>();
        private BidirectionalGraph<object, STaggedEdge<object, object>> _sourceGraph;

        public Single this[object from, object to] { get => FindDsmElementByGraphVertex(from, to); set => SetDsmElementByGraphVertex(from, to, value);  }

        public List<object> Objects { get => GetDsmElementNames(); }
        public int Size => this.Objects.Count;
        public int Count => (this._dsm.RowCount * this._dsm.ColumnCount);

        public Matrix<float> Dsm { get => _dsm; }

        public Dictionary<object, int> RowIndex => _rowIndex;
        public Dictionary<object, int> ColIndex => _colIndex;


        public Single GetByIndex(int i, int j)
        {
            return _dsm[i, j];
        }


        private void SetDsmElementByGraphVertex(object from, object to, float value)
        {
            var colIndex = _colIndex[from];
            var rowIndex = _rowIndex[to];

            _dsm[rowIndex, colIndex] = value;

            //throw new NotImplementedException();
        }

        public DsmStorage GetOrderedDsm(List<Cluster> clusters) 
        {
            Dictionary<object, int> rowIndex = new Dictionary<object, int>();
            Dictionary<object, int> colIndex = new Dictionary<object, int>();
            List<object> index = new List<object>();
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

        public void SwapRowsByObject(object source, object target)
        {
            var sourceIdx = _rowIndex[source];
            var targetIdx = _rowIndex[target];

            SwapRows(sourceIdx, targetIdx);
        }

        public void SwapColumnsByObject(object source, object target)
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

        private List<object> GetDsmElementNames()
        {
            return _colIndex.Keys.ToList();
        }

        private float FindDsmElementByGraphVertex(object from, object to)
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
        public DsmStorage(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            _sourceGraph = graph;
            _dsm = GraphToDSM();
        }

        protected DsmStorage(Matrix<Single> dsm,
                             Dictionary<object, int> rowIndex,
                             Dictionary<object, int> colIndex,
                             BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            _sourceGraph = graph;
            _dsm = dsm;
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }
        #endregion constructors

    }
}

