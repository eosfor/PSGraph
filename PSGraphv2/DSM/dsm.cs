using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;

namespace PSGraph.DSM
{
    public class dsm
    {
        private int[,] _dsm;
        public int Size = 0;
        private Dictionary<object, int> _colIndex = new Dictionary<object, int>();
        private Dictionary<object, int> _rowIndex = new Dictionary<object, int>();

        public int this[int row, int col] { get => _dsm[row, col]; }

        public dsm(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            _dsm = GraphToDSM(graph);


        }
        private int[,] GraphToDSM(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            int[,] dsm =  new int[graph.VertexCount, graph.VertexCount];



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

                var rowIndex = _rowIndex[from];
                var colIndex = _colIndex[to];
                dsm[rowIndex, colIndex] = 1;

            }

            Size = dsm.GetLength(0);
            return dsm;
        }

        public void Cluster()
        {
            throw new NotImplementedException();
        }
    }
}
