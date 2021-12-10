using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using Svg;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace PSGraph.DesignStructureMatrix
{
    public class Cluster
    {
        public List<object> Objects = new List<object>();

        //public System.Drawing.Color Color = System.Drawing.Color.FromArgb(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255));

    }
    public class Dsm
    {
        //private int[,] _dsm;
        private BidirectionalGraph<object, STaggedEdge<object, object>> _sourceGraph;
        private DsmStorage _dsm;
        private List<Cluster> _clusters;


        private int _pow_cc = 2;
        private int _pow_bid = 2;
        private int _pow_dep = 2;
        private int _max_Cl_size = 0;
        private int _rand_accept = 0;
        private int _rand_bid = 0;
        private int _times = 2;
        private int _stable_limit = 2;

        public Single this[int row, int col] { get => _dsm[row, col]; }
        public int Count => _dsm.Count;
        public int Size => _dsm.Size;

        protected Dsm(BidirectionalGraph<object, STaggedEdge<object, object>> sourceGraph, 
                      DsmStorage dsm, 
                      List<Cluster> clusters,
                      int pow_cc = 2,
                      int pow_bid = 2,
                      int max_Cl_size = 0,
                      int rand_accept = 0,
                      int rand_bid = 0,
                      int times = 2,
                      int stable_limit = 2)
        {
            _sourceGraph = sourceGraph;
            _dsm = dsm;
            _clusters = clusters;
            _pow_cc = pow_cc;
            _pow_bid = pow_bid;
            _max_Cl_size = max_Cl_size;
            _rand_accept = rand_accept;
            _rand_bid = rand_bid;
            _times = times;
            _stable_limit = stable_limit;
        }

        public Dsm(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            var r = new Random();
            _sourceGraph = graph;
            _dsm = GraphToDSM(_sourceGraph);
            _max_Cl_size = _dsm.Size;
            _rand_accept = r.Next() % 2 == 0 ? (int)Math.Round((decimal)_dsm.Size / 2) : _max_Cl_size * 2;
            _rand_bid = r.Next() % 2 == 0 ? (int)Math.Round((decimal)_dsm.Size / 2) : _max_Cl_size * 2;
        }

        private DsmStorage GraphToDSM(BidirectionalGraph<object, STaggedEdge<object, object>> graph)
        {
            return new DsmStorage(graph);

        }

        public void Cluster()
        {

            bool isStable = false;
            int system = 0;

            CreateInitialClusters();
            var tCCost = CalculateTotalCoordinationCost();

            while (system < _stable_limit)
            {
                for (int k = 0; k <= _dsm.Size * _times; k++)
                {
                    var obj = PickRamdomDsmObject();
                    var bid = CalculateClusterBids(obj);
                    var bestBid = bid.Last().Value[0];
                    var oldCluster = Move(obj, bestBid);
                    var newTCCost = CalculateTotalCoordinationCost();
                    if (newTCCost <= tCCost)
                    {
                        tCCost = newTCCost;
                    }
                    else
                    {
                        var r = new Random();
                        int choice = -1;

                        // rolling a dice from 0 to _rand_accept
                        if (_rand_accept > 0)
                        {
                            choice = r.Next(_rand_accept);
                        }

                        // in one case out of _rand_accept possibilities we keep changes
                        // otherwise - rollback
                        if (choice != _rand_accept)
                        {
                            foreach (var cluster in oldCluster)
                            {
                                Move(obj, cluster);
                            }
                            
                        }
                        
                    }
                    UpdateClusters();
                }
                system++;
            }
        }


        public Dsm Order()
        {
            var d = _dsm.GetOrderedDsm(_clusters);
            var x = new Dsm(_sourceGraph, d, _clusters ,_pow_cc, _pow_bid, _max_Cl_size, _rand_accept, _rand_bid, _times, _stable_limit);

            return x;
        }

        public void ExportSvg(string Path)
        {
            var svgDoc = new SvgDocument()
            {
                Width = _dsm.Size * 10 + 200,
                Height = _dsm.Size * 10 + 200,
                ViewBox = new SvgViewBox() { MinX = 0, MinY = 0, Height = _dsm.Size * 10 + 200, Width = _dsm.Size * 10 + 200 }
            };

            var svgGroup = new SvgGroup();
            svgDoc.Children.Add(svgGroup);

            var elementNames = _dsm.Objects;

            for (int i = 0; i <= _dsm.Size - 1; i++)
            {
                var row = i * 10 + 100;
                for (int j = 0; j <= _dsm.Size - 1; j++)
                {
                    var col = j * 10 + 100;
                    var rect = new SvgRectangle()
                    {
                        Width = 10,
                        Height = 10,
                        X = row,
                        Y = col,
                        StrokeWidth = 1,
                        Stroke = new SvgColourServer(System.Drawing.Color.Black)
                    };
                    if (_dsm.GetByIndex(i, j) == 0)
                    {
                        rect.Fill = new SvgColourServer(System.Drawing.Color.White);
                    }
                    else
                    {
                        rect.Fill = new SvgColourServer(System.Drawing.Color.Red);
                    }

                    svgDoc.Children.Add(rect);

                }
            }

            svgDoc.Write(Path);
        }

        private void UpdateClusters()
        {
            RemoveEmptyClusters();
            RemoveIdenticalClusters();
            //throw new NotImplementedException();
        }

        private void RemoveIdenticalClusters()
        {
            for (int k = 0; k < _clusters.Count; k++)
            {
                for (int i = k + 1; i < _clusters.Count; i++)
                {
                    if (_clusters[i] == _clusters[k])
                    {
                        _clusters.RemoveAt(i);
                    }
                }
            }
            //throw new NotImplementedException();
        }

        private void RemoveEmptyClusters()
        {

            var r = _clusters.RemoveAll( c => c.Objects.Count == 0);
            //throw new NotImplementedException();
        }

        private List<Cluster> Move(object obj, Cluster bestBid)
        {
            var oldClusters = FindDsmObjectCluster(obj);
            
            foreach (var cluster in oldClusters)
            {
                cluster.Objects.Remove(obj);
            }

            bestBid.Objects.Add(obj);

            return oldClusters;
            //throw new NotImplementedException();
        }

        private SortedDictionary<double, List<Cluster>> CalculateClusterBids(object obj)
        {
            double bid;
            var ret = new SortedDictionary<double, List<Cluster>>();

            foreach (var cluster in _clusters)
            {
                bid = 0;
                foreach (var dsmObject in cluster.Objects)
                {
                    if (dsmObject != obj) // avoid diagonal elements
                    {
                        var a = _dsm[obj, dsmObject];
                        var b = _dsm[dsmObject, obj];
                        bid += Math.Pow(a + b, _pow_dep) / Math.Pow(cluster.Objects.Count, _pow_bid);

                    }
                }


                if (ret.ContainsKey(bid)) {
                    ret[bid].Add(cluster);
                }
                else {
                    var l = new List<Cluster>();
                    l.Add(cluster);
                    ret.Add(bid, l);
                }
                
            }

            return ret;
        }

        private object PickRamdomDsmObject()
        {
            var r = new Random();
            var idx = r.Next(_dsm.Size - 1);

            return _dsm.Objects[idx];

            //throw new NotImplementedException();
        }

        private float CalculateTotalCoordinationCost()
        {
            float intraClustertCost = 0;
            float extraClusterCost = 0;
            float cl_size = 0;

            foreach (var item in _clusters)
            {
                cl_size += (float)(Math.Pow(item.Objects.Count, _pow_cc));
            }

            foreach (var obj1 in _dsm.Objects)
            {
                var cluster1 = FindDsmObjectCluster(obj1);
                foreach (var obj2 in _dsm.Objects)
                {
                    var cluster2 = FindDsmObjectCluster(obj2);

                    var dsmSum = _dsm[obj1, obj2] + _dsm[obj2, obj1];
                    if (cluster1.SequenceEqual(cluster2)) // what if any of these clusters have more than one list?
                    {
                        // sum intra cluster cost
                        intraClustertCost += dsmSum * cl_size;
                    }
                    else
                    {
                        // sum extra cluster cost
                        extraClusterCost += (float)(dsmSum * Math.Pow(_dsm.Size, _pow_cc));
                    }
                }
            }

            return intraClustertCost + extraClusterCost;
        }

        private List<Cluster> FindDsmObjectCluster(object obj)
        {
            return _clusters.Where(l => l.Objects.Contains(obj)).ToList();
        }

        private void CreateInitialClusters()
        {
            _clusters = new List<Cluster>();
            foreach (var item in _dsm.Objects)
            {
                var cluster = new Cluster();
                cluster.Objects.Add(item);
                _clusters.Add(cluster);
            }
            //throw new NotImplementedException();
        }
    }
}
