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
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix
{
    public class Cluster
    {
        public List<object> Objects = new List<object>();
        public System.Drawing.Color Color = System.Drawing.Color.FromArgb(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255));

        public override bool Equals(object? obj)
        {
            bool res = false;

            if (null != obj)
            {
                if (((Cluster)obj).Objects.Count == Objects.Count)
                {
                    foreach (var item in ((Cluster)obj).Objects)
                    {
                        if (Objects.Contains(item)) { res = true; break; }
                        else { res = false; }
                    }
                }

            }

            return obj is Cluster cluster &&
                   res;//EqualityComparer<List<object>>.Default.Equals(Objects, cluster.Objects) &&
        }
    }
    public class Dsm
    {
        private PSBidirectionalGraph _sourceGraph;
        private DsmStorage _dsm;
        private List<Cluster> _clusters;


        private int _pow_cc = 2;
        private double _pow_bid = 0.5;
        private int _pow_dep = 2;
        private int _max_Cl_size = 0;
        private int _rand_accept = 0;
        private int _rand_bid = 0;
        private int _times = 2;
        private int _stable_limit = 10;

        private Stack<SortedDictionary<double, List<Cluster>>> _historicalBids = new Stack<SortedDictionary<double, List<Cluster>>>();

        private float _minObservedtCCost = float.MaxValue ;
        public float MinObservedtCCost { get => _minObservedtCCost; }
        private float _tCCost = 0;

        public Single this[int row, int col] { get => _dsm[row, col]; }
        public int Count => _dsm.Count;
        public int Size => _dsm.Size;

        private List<Cluster> FindClusterByCoordinates(int i, int j)
        {
            var from = _dsm.ColIndex.Where(x => x.Value == i).First();
            var to = _dsm.RowIndex.Where(e => e.Value == i).First();

            var ret = _clusters.Where(c => c.Objects.Contains(from.Key) && c.Objects.Contains(to.Key)).ToList();

            return ret;
        }

        protected Dsm(PSBidirectionalGraph sourceGraph, 
                      DsmStorage dsm, 
                      List<Cluster> clusters,
                      int pow_cc = 2,
                      double pow_bid = 1,
                      int max_Cl_size = 0,
                      int rand_accept = 0,
                      int rand_bid = 0,
                      int times = 5,
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

        public Dsm(PSBidirectionalGraph graph)
        {
            var r = new Random();
            _sourceGraph = graph;
            _dsm = GraphToDSM(_sourceGraph);
            _max_Cl_size = _dsm.Size;
            _rand_accept = r.Next() % 2 == 0 ? (int)Math.Round((decimal)_dsm.Size / 2) : _max_Cl_size * 2;
            _rand_bid = r.Next() % 2 == 0 ? (int)Math.Round((decimal)_dsm.Size / 2) : _max_Cl_size * 2;
        }

        private DsmStorage GraphToDSM(PSBidirectionalGraph graph)
        {
            return new DsmStorage(graph);

        }

        public void Cluster()
        {

            int system = 0;
            var r = new Random();

            CreateInitialClusters();
            _tCCost = CalculateTotalCoordinationCost();

            while (system < _stable_limit)
            {
                for (int k = 0; k <= _dsm.Size * _times; k++)
                {
                    var obj = PickRamdomDsmObject();
                    var bid = CalculateClusterBids(obj);
                    _historicalBids.Push(bid);

                    var currentRandBid = r.Next(_rand_bid - 1);
                    var bestBid = currentRandBid == (_rand_bid - 1) ? bid.Skip(bid.Count - 2).First().Value[0] : bid.Last().Value[0];

                    var oldCluster = Move(obj, bestBid);
                    var newTCCost = CalculateTotalCoordinationCost();

                    var currentRandAccept = r.Next(_rand_accept - 1);
                    //system = newTCCost == _tCCost ? system+1 : system;
                    if ( newTCCost <= _tCCost  )
                    {
                        system = newTCCost == _tCCost ? system + 1 : 0;
                        _tCCost = newTCCost;
                    }
                    else
                    {
                        if (currentRandAccept == (_rand_accept - 1)) {
                            // rollback
                            foreach (var cluster in oldCluster)
                            {
                                Move(obj, cluster);
                            }
                            system += 1;
                        }
                        else
                        {
                            _tCCost = newTCCost;
                            system = 0;
                        }

                    }
                    UpdateClusters();
                }
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
            int textShift = 15;
            int itemSize = 5;
            var svgDoc = new SvgDocument()
            {
                Width = _dsm.Size * itemSize + textShift,
                Height = _dsm.Size * itemSize + textShift,
                ViewBox = new SvgViewBox() { MinX = 0, MinY = 0, Height = _dsm.Size * itemSize + textShift, Width = _dsm.Size * itemSize + textShift }
            };

            var svgGroup = new SvgGroup();
            svgDoc.Children.Add(svgGroup);

            var elementNames = _dsm.Objects;

            for (int i = 0; i <= _dsm.Size - 1; i++)
            {
                var row = i * itemSize + textShift;
                for (int j = 0; j <= _dsm.Size - 1; j++)
                {
                    var cl = FindClusterByCoordinates(i, j).First();

                    var col = j * itemSize + textShift;
                    var rect = new SvgRectangle()
                    {
                        Width = itemSize,
                        Height = itemSize,
                        X = row,
                        Y = col,
                        StrokeWidth = (float)0.5,
                        Stroke = new SvgColourServer(System.Drawing.Color.DimGray)
                    };
                    if (i != j)
                    {
                        if (_dsm.GetByIndex(j, i) == 0)
                        {
                            rect.Fill = new SvgColourServer(System.Drawing.Color.White);
                        }
                        else
                        {
                            rect.Fill = new SvgColourServer(cl.Color); /*new SvgColourServer(System.Drawing.Color.Red);*/
                        }
                    }
                    else
                    {
                        rect.Fill = new SvgColourServer(System.Drawing.Color.SlateGray);
                    }

                    svgDoc.Children.Add(rect);

                }
            }

            // cluster bondaries
            foreach(var cluster in _clusters)
            {
                var row = _dsm.RowIndex[cluster.Objects.First()] * itemSize + textShift;
                var w = cluster.Objects.Count * itemSize;
                var h = cluster.Objects.Count * itemSize;

                var rect = new SvgRectangle()
                {
                    Width = w,
                    Height = h,
                    X = row,
                    Y = row, //top left corner
                    StrokeWidth = (float)1.0,
                    Stroke = new SvgColourServer(System.Drawing.Color.Black),
                    FillOpacity = 0
                };

                svgDoc.Children.Add(rect);
            }

            // labels - X
            foreach (var row in _dsm.RowIndex)
            {
                var lbl = new Svg.SvgText(row.Key.ToString());
                lbl.X.Add(textShift - itemSize);
                lbl.Y.Add((row.Value + 1) * itemSize + textShift - 1);
                lbl.FontSize = itemSize - 1;
                svgDoc.Children.Add(lbl);
            }

            // labels - Y
            foreach (var col in _dsm.ColIndex)
            {
                var lbl = new Svg.SvgText(col.Key.ToString());
                lbl.X.Add((col.Value) * itemSize + textShift);
                lbl.Y.Add(textShift - itemSize + 2);
                lbl.FontSize = itemSize - 1;
                svgDoc.Children.Add(lbl);
            }

            svgDoc.Write(Path);
        }

        private void UpdateClusters()
        {
            RemoveEmptyClusters();
            RemoveIdenticalClusters();
        }

        private void RemoveIdenticalClusters()
        {
            for (int k = 0; k < _clusters.Count; k++)
            {
                for (int i = k + 1; i < _clusters.Count; i++)
                {
                    if (_clusters[i].Equals(_clusters[k]))
                    {
                        _clusters.RemoveAt(i);
                    }
                }
            }
        }

        private void RemoveEmptyClusters()
        {

            var r = _clusters.RemoveAll( c => c.Objects.Count == 0);
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

            var ret = intraClustertCost / extraClusterCost;
            _minObservedtCCost = ret < _minObservedtCCost ? ret : _minObservedtCCost; //want to record the minimum cost to compare it to the selected one at the end of the run

            return ret;
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
