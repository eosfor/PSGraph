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
    public class Dsm
    {
        private PSBidirectionalGraph _sourceGraph;
        private DsmStorage _dsm;
        private List<DSMCluster> _clusters;

        // algo parameters
        private int _pow_cc = 2;
        private double _pow_bid = 0.5;
        private int _max_Cl_size = 0;
        private int _rand_accept = 4;
        private int _rand_bid = 0;
        private int _times = 7;
        private int _stable_limit = 10;
        private double _tCCost = 0;

        // some stats
        private double _minObservedtCCost = double.MaxValue;
        private int _rollbackCount = 0;

        private int _system = 0;
        private int _runCount = 0;

        private Stack<SortedDictionary<double, List<DSMCluster>>> _historicalBids = new Stack<SortedDictionary<double, List<DSMCluster>>>();
        private Stack<ValueTuple<double, int>> _historicalTCC = new Stack<ValueTuple<double, int>>();
        private Stack<int> _historicalStableLimit = new Stack<int>();

        public double MinObservedtCCost { get => _minObservedtCCost; }
        public Stack<ValueTuple<double, int>> HistoricalTCC { get => _historicalTCC; }
        public Stack<SortedDictionary<double, List<DSMCluster>>> HistoricalBids { get => _historicalBids; }
        public Stack<int> HistoricalStableLimit { get => _historicalStableLimit; } 
        public int RunCount { get => _runCount; }

        public int RollbackCount { get => _rollbackCount; }

        public Single this[PSVertex row, PSVertex col] { get => _dsm[row, col]; }
        public int Count => _dsm.Count;
        public int Size => _dsm.Size;
        public List<DSMCluster> Clusters => _clusters;

        public Matrix<float> DsmStorage { get => _dsm.Dsm; }

        public Dictionary<PSVertex, int>  RowIndex => _dsm.RowIndex;
        public Dictionary<PSVertex, int> ColIndex => _dsm.ColIndex;


        private List<DSMCluster> FindClusterByCoordinates(int i, int j)
        {
            var from = _dsm.ColIndex.Where(x => x.Value == i).First();
            var to = _dsm.RowIndex.Where(e => e.Value == i).First();

            var ret = _clusters.Where(c => c.Objects.Contains(from.Key) && c.Objects.Contains(to.Key)).ToList();

            return ret;
        }

        private DsmStorage GraphToDSM(PSBidirectionalGraph graph)
        {
            return new DsmStorage(graph);
        }


        public float[] Row(PSVertex obj)
        {
            return _dsm.Row(obj);
        }

        public float[] Column(PSVertex obj)
        {
            return _dsm.Column(obj);
        }

        public PSBidirectionalGraph GetClusteredViewGraph()
        {
            if (_clusters == null) throw new InvalidOperationException("The DSM hasn't been clustered yet. Please run the corresponding command first");
            var ret = new PSBidirectionalGraph();

            foreach (var c in _clusters)
            {
                var edges = c.GetOutEdges(_clusters, this);

                if(edges.Count == 0)
                {
                    ret.AddVertex(new PSVertex(c.ToString(), c));
                    continue;
                }

                foreach (var e in edges)
                {
                    ret.AddVerticesAndEdge(new PSEdge(new PSVertex(c.ToString(), c), new PSVertex(e.ToString(), e), new PSEdgeTag()) );
                }
            }

            return ret;
        }

        public void Cluster()
        {
            var r = new Random();

            if (null == _clusters && _runCount == 0)
            {
                CreateInitialClusters();
            }

            //if (_runCount > 0)
            //{
            //    _system = 0; //reset the stability counter for cases when one feeds the same DSM again
            //}

            _runCount += 1;
            _tCCost = CalculateTotalCoordinationCost();

            while (_system < _stable_limit)
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

                    if ( newTCCost <= _tCCost  )
                    {
                        _system = newTCCost == _tCCost ? _system + 1 : 0;
                        _tCCost = newTCCost;
                        _historicalStableLimit.Push(_system);
                    }
                    else
                    {
                        if (currentRandAccept == (_rand_accept - 1)) {
                            // rollback
                            Move(obj, oldCluster);
                            _system += 1;
                            _historicalStableLimit.Push(_system);
                            ++_rollbackCount;
                        }
                        else
                        {
                            _tCCost = newTCCost;
                            _system = 0;
                            _historicalStableLimit.Push(_system);
                        }

                    }
                    UpdateClusters();
                }
            }
        }


        public Dsm Order()
        {
            var d = _dsm.GetOrderedDsm(_clusters);
            var x = new Dsm(this, d);

            return x;
        }

        public void ExportSvg(string Path)
        {
            if (null == _clusters && _runCount == 0)
            {
                CreateInitialClusters();
            }
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
                        X = col,//row,
                        Y = row,//col,
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

        private DSMCluster Move(PSVertex obj, DSMCluster bestBid)
        {
            var oldCluster = FindDsmObjectCluster(obj);
            oldCluster.Objects.Remove(obj);
            bestBid.Objects.Add(obj);

            return oldCluster;
        }

        private SortedDictionary<double, List<DSMCluster>> CalculateClusterBids(PSVertex obj)
        {
            double bid = 0;
            var ret = new SortedDictionary<double, List<DSMCluster>>();

            foreach (var cluster in _clusters)
            {
                bid += cluster.Bid(obj, _dsm);

                if (ret.ContainsKey(bid)) {
                    ret[bid].Add(cluster);
                }
                else {
                    var l = new List<DSMCluster>();
                    l.Add(cluster);
                    ret.Add(bid, l);
                }
                
            }

            return ret;
        }

        private PSVertex PickRamdomDsmObject()
        {
            var r = new Random();
            var idx = r.Next(_dsm.Size - 1);

            return _dsm.Objects[idx];
        }

        private double CalculateTotalCoordinationCost()
        {
            double tcc = 0;

            foreach (var cluster in _clusters)
            {
                foreach (var o in _dsm.Objects)
                {
                    tcc += cluster.CoordinationCost(o, _dsm);
                }
            }

            //want to record the minimum cost to compare it to the selected one at the end of the run
            _minObservedtCCost = tcc < _minObservedtCCost ? tcc : _minObservedtCCost;
            _historicalTCC.Push((tcc,_system));

            return tcc;
        }

        public DSMCluster FindDsmObjectCluster(object obj)
        {
            return _clusters.Where(l => l.Objects.Contains(obj)).First();
        }

        private void CreateInitialClusters()
        {
            _clusters = new List<DSMCluster>();
            foreach (var item in _dsm.Objects)
            {
                var cluster = new DSMCluster();
                cluster.Objects.Add(item);
                _clusters.Add(cluster);
            }
        }

        #region constructors
        public Dsm(Dsm dsm, DsmStorage newStorage)
        {
            _dsm = newStorage;
            
            _clusters = dsm._clusters;
            _sourceGraph = dsm._sourceGraph;
            _pow_cc = dsm._pow_cc;
            _pow_bid = dsm._pow_bid;
            _max_Cl_size = dsm._max_Cl_size;
            _rand_accept = dsm._rand_accept;
            _rand_bid = dsm._rand_bid;
            _times = dsm._times;
            _stable_limit = dsm._stable_limit;
            _minObservedtCCost = dsm._minObservedtCCost;
            _rollbackCount = dsm._rollbackCount;
            _runCount = dsm._runCount;
            //_system = dsm._system; do not copy to be abe to restart clustering process

            // TODO: need to get rid of cloning; perhaps it is better to reshuffle the matrix inplace
            _historicalTCC = dsm._historicalTCC;
            _historicalStableLimit = dsm._historicalStableLimit;
            _historicalBids = dsm._historicalBids;
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

        #endregion constructors
    }
}
