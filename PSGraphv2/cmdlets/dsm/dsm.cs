using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using MathNet.Numerics.LinearAlgebra;

namespace PSGraph.DSM
{
    public class dsm
    {
        //private int[,] _dsm;
        private Matrix<Single> _dsm;
        public int Size = 0;
        private Dictionary<object, int> _colIndex = new Dictionary<object, int>();
        private Dictionary<object, int> _rowIndex = new Dictionary<object, int>();

        public Single this[int row, int col] { get => _dsm[row, col]; }

        public dsm(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            _dsm = GraphToDSM(graph);
        }

        public dsm(Matrix<Single> m)
        {
            _dsm = m;
            Size = m.RowCount;
        }

        private Matrix<Single> GraphToDSM(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
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

                var rowIndex = _rowIndex[from];
                var colIndex = _colIndex[to];
                dsm[rowIndex, colIndex] = 1;

            }

            Size = dsm.RowCount;
            return dsm;
        }

        public void Cluster()
        {
            int powcc = 1;
            int powbid = 0;
            int powdep = 1;
            int rand_accept = 30;
            int rand_bid = 30;
            int times = 1;
            int stable_limit = 2;

            Matrix<Single> Clustermatrix = Matrix<Single>.Build.DenseIdentity(Size);
            Matrix<Single> cDSM  = Matrix<Single>.Build.Dense(Size, Size);
            _dsm.CopyTo(cDSM);

            var TCCost = TCC(cDSM, Clustermatrix, powcc);

            double[] costlist = new double[Size * times];

            for (int i = 0; i < Size * times; i++)
            {
                Matrix<Single> preClustermatrix = Matrix<Single>.Build.Dense(Size, Size);
                int element = Random.Shared.Next(1, Size);
                var bestCLuter = ClusterBid(Clustermatrix, cDSM, powdep, powbid, element);
                foreach (var row in Clustermatrix.EnumerateRows())
                {
                    for (int k = 0; k <= element; k++)
                    {
                        row[k] = 0;
                    }
                }

                Clustermatrix[bestCLuter, element] = 1;
                var newCost = TCC(cDSM, Clustermatrix, powcc);

                if (newCost <= Math.Max(TCCost, rand_accept))
                {
                    TCCost = newCost;
                }
                else
                {
                    Clustermatrix = preClustermatrix;
                }

                costlist[i] = newCost;

            }

            int xxx = 1;
            //throw new NotImplementedException();
        }

        private int ClusterBid(Matrix<Single> Clustermatrix, Matrix<Single> cDSM, Single powdep, Single powbid, int element)
        {
            int bestCluster = 0;
            double bestBid = -1;

            for (int i = 0; i < Size; i++)
            {
                var Clusterlist = FindAllOnes(Clustermatrix);
                int Clustersize = Clusterlist.Length;
                double bid = 0;

                for (int j = 0; j< Clustersize; j++)
                {
                    bid += cDSM[element, (int)Clusterlist[j]];
                }

                bid = Math.Pow(bid, powdep) / Math.Pow(Clustersize, powbid);

                if (bid > bestBid)
                {
                    bestBid = bid;
                    bestCluster = i;
                }
            }

            return bestCluster;
        }

        private double TCC(Matrix<Single> cDSM, Matrix<Single> Clustermatrix, int powcc)
        {
            double intraCost = 0;
            double extraCost = 0;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    var Clusterofi = FindAllOnes(Clustermatrix, i);
                    var Clusterofj = FindAllOnes(Clustermatrix, j);

                   int Clustersizeofi = Clusterofi.Length;


                    var cost = cDSM[i, j] + cDSM[j, i];

                    if (Clusterofi == Clusterofj)
                    {
                        intraCost += cost * Math.Pow(Clustersizeofi, powcc);
                    }
                    else
                    {
                        extraCost += cost * Math.Pow(Size, powcc);
                    }
                }
            }

            return intraCost + extraCost;
        }

        private Single[] FindAllOnes(Matrix<Single> clustermatrix)
        {
            return clustermatrix.Enumerate(Zeros.AllowSkip).ToList().ToArray();
        }

        private Single[] FindAllOnes(Matrix<Single> clustermatrix, int column)
        {
            List<Single> result = new List<Single>();
            foreach (var row in clustermatrix.EnumerateRows())
            {
                for (int k = 0; k < column; k++)
                {
                    if (row[k] == 1)
                    {
                        result.Add(row[k]);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
