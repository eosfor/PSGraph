using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSGraph.DesignStructureMatrix;

public class DsmSimulatedAnnealingAlgorithm : IDsmPartitionAlgorithm
{
    IDsm _dsm;
    IDsm _bestDsm;
    IDictionary<int, List<PSVertex>> _bestClusterLayout;

    IDictionary<int, List<PSVertex>> _clusters;
    Dictionary<UnorderedPair<PSVertex>, double> _pairs;
    Dictionary<int, int> _improvementStats; // статистика процесса улучшения
    private int _passes, _stableStateCount;
    private double _tcc, _bestTcc;
    public IDsm Partitioned => _dsm;

    public Dictionary<int, int> ImprovementStats => _improvementStats;

    public List<List<PSVertex>> Partitions => _clusters.Values.ToList();

    // configuration (directly from original algo + annealing additions)
    private int _powCc = 1;     // penalty assigned to cluster size
    private int _powBid = 0;    // high value penalizes large clusters
    private int _powDep = 0;    // high value emphasizes high interactions
    private int _maxClusterSize = 61;   // max size of cluster(DSM size)
    private int _randAccept = 122;  // legacy random accept (now superseded by temperature but retained as fallback)
    private int _randBid = 122; // take second highest bid 1 out of N times(0.5-2 * DSM)
    private int _times = 2;     // attempt times*size before check sys.stability (inner moves per pass scalar)
    private int _stableLimit = 2;       // loop at least stable_limit*times* size
    private int _maxRepeat = 1000;      // safety cap on passes
    // Simulated annealing parameters
    private double _temperature;        // current temperature
    private double _coolingRate = 0.95; // multiplicative cooling per pass
    private double _minTemperature = 1e-3; // termination threshold
    private double? _initialTemperatureOverride; // user-specified initial temperature
    private int _temperatureSampleMoves = 0; // reserved for future adaptive T0 (not yet used)

    // cost tracking
    private readonly List<double> _costHistory = new();
    public IReadOnlyList<double> CostHistory => _costHistory;

    private readonly List<double> _temperatureHistory = new();
    public IReadOnlyList<double> TemperatureHistory => _temperatureHistory;
    public double BestCost => _bestTcc;

    // NOTE: legacy internal Config replaced by public DsmSimulatedAnnealingConfig (see Model folder)

    double AbsEps = 1e-12;
    double RelEps = 1e-9;

    public DsmSimulatedAnnealingAlgorithm(IDsm dsm, DsmSimulatedAnnealingConfig? cfg = null)
    {
        _dsm = _bestDsm = dsm;
        _clusters = _bestClusterLayout = new Dictionary<int, List<PSVertex>>();
        _pairs = new();
        _improvementStats = new();

        if (cfg is not null)
        {
            _powCc = cfg.PowCc;
            _powBid = cfg.PowBid;
            _powDep = cfg.PowDep;
            _times = cfg.Times;
            _stableLimit = cfg.StableLimit;
            _maxRepeat = cfg.MaxRepeat;
            _initialTemperatureOverride = cfg.InitialTemperature;
            _coolingRate = cfg.CoolingRate;
            _minTemperature = cfg.MinTemperature;
        }

        _passes = 0;  //number of passes
        _stableStateCount = 0;
        _maxClusterSize = _dsm.DsmMatrixView.RowCount; // матрица квадратная, так что можно взять только количество строк
        _randAccept = _dsm.DsmMatrixView.RowCount * 2;
        _randBid = _dsm.DsmMatrixView.RowCount * 2;
    }



    public IDsm Partition()
    {
        Initialize();
        InitializeTemperature();

        while (!IsConverged())
        {
            bool acceptedThisPass = false;
            bool improvedThisPass = false;
            _improvementStats[_passes] = 0;

            for (int i = 0; i < (_times * _maxClusterSize); i++)
            {
                var element = SelectRandomElement();
                var bidVectors = Bid(element).ToList();

                // на случай если вернется 0 элементов
                if (bidVectors.Count == 0) continue;

                var bestBid = SelectBestBid(bidVectors);

                int oldClusterId = Convert.ToInt32(element.Metadata["cluster"]); // сохраняем старый кластер для отката

                var success = MoveVertex(bestBid.Item1, element);
                if (!success) continue;

                var newTcc = CalculateTotalCoordinationCost();

                var eps = Math.Max(AbsEps, RelEps * Math.Abs(_tcc));
                if (newTcc < _tcc - eps)
                {
                    // Strict improvement always accepted
                    acceptedThisPass = true;
                }
                else
                {
                    // Metropolis acceptance: accept worse with probability exp(-Δ/T)
                    double delta = newTcc - _tcc;
                    if (delta > 0)
                    {
                        // guard against overflow in exp
                        double x = -delta / _temperature;
                        if (x > 700) x = 700; // prevent double overflow
                        double prob = Math.Exp(x);
                        acceptedThisPass = Random.Shared.NextDouble() < prob;
                    }
                    else
                    {
                        acceptedThisPass = true;
                    }
                    // else
                    // {
                    //     // fallback to legacy random accept when temperature exhausted
                    //     acceptedThisPass = Random.Shared.Next(_randAccept) == 0;
                    // }
                }

                if (acceptedThisPass)
                {
                    _tcc = newTcc;
                    _costHistory.Add(_tcc);

                    eps = Math.Max(AbsEps, RelEps * Math.Abs(_bestTcc));
                    if (_tcc < _bestTcc - eps)
                    {
                        _bestDsm = _dsm.Clone();
                        _bestClusterLayout = _clusters.ToDictionary(
                                            kvp => kvp.Key,
                                            kvp => new List<PSVertex>(kvp.Value)
                                        );
                        _bestTcc = _tcc;
                        improvedThisPass = true;
                        _improvementStats[_passes]++;
                    }

                }
                else
                {
                    // откат
                    success = MoveVertex(oldClusterId, element);
                }

                // Cool temperature after each pass, save it for later review
                _temperatureHistory.Add(_temperature);
                _temperature *= _coolingRate;
            }

            _passes++;
            if (!improvedThisPass) _stableStateCount++; else _stableStateCount = 0;

            int removedCount = RemoveEmptyClusters(_clusters);
            // record pass boundary cost (even if no move accepted)
            if (_costHistory.Count == 0 || _costHistory[^1] != _tcc)
                _costHistory.Add(_tcc);
        }

        IDsm dsmToReturn;
        IDictionary<int, List<PSVertex>> clusterLayoutToReturn;
        if (_bestTcc < _tcc)
        {
            dsmToReturn = _bestDsm;
            clusterLayoutToReturn = _bestClusterLayout;
        }
        else
        {
            dsmToReturn = _dsm;
            clusterLayoutToReturn = _clusters;
        }

        List<PSVertex> newOrder = clusterLayoutToReturn.SelectMany(e => e.Value).ToList();
        dsmToReturn = (IDsm)dsmToReturn.Order(newOrder); // apply immutable ordering
        _dsm = dsmToReturn; // sync
        return _dsm;
    }

    public PartitioningExtendedResult PartitionWithDetails()
    {
        var dsm = Partition();
        return new PartitioningExtendedResult
        {
            Dsm = dsm,
            Algorithm = this,
            CostHistory = CostHistory,
            TemperatureHistory = TemperatureHistory,
            ImprovementStats = new Dictionary<int, int>(ImprovementStats),
            BestCost = BestCost,
            Passes = _passes,
            StablePasses = _stableStateCount
        };
    }

    private int RemoveEmptyClusters(IDictionary<int, List<PSVertex>> clusters)
    {
        int removedCount = 0;
        var idsToRemove = clusters.Where(c => c.Value.Count == 0).Select(c => c.Key).ToList();
        foreach (var item in idsToRemove)
        {
            clusters.Remove(item);
            removedCount++;
        }

        return removedCount;
    }

    private bool MoveVertex(int targetClusterId, PSVertex element)
    {
        var metadata = element.Metadata as IDictionary<string, object?>;
        if (metadata == null || !metadata.ContainsKey("cluster"))
        {
            throw new InvalidDataException($"cluster node  {element} do not have metadata[\"cluster\"] property set");
        }

        int sourceClusterId = Convert.ToInt32(metadata["cluster"]);

        if (sourceClusterId == targetClusterId) return false;

        if (_clusters[sourceClusterId].Remove(element))
        {
            _clusters[targetClusterId].Add(element);
            metadata["cluster"] = targetClusterId;
            return true;
        }
        else
        {
            return false;
        }
    }


    // TODO: добавить полноценную симуляцию отжига с Т0, расписанием охлаждение и т.д.
    private (int, double) SelectBestBid(IEnumerable<(int, double)> bidVectors)
    {
        var top2 = bidVectors
            .OrderByDescending(x => x.Item2)
            .Take(2)
            .ToList();

        var max = top2.ElementAtOrDefault(0);      // максимальный
        var next = top2.ElementAtOrDefault(1);     // следующий за ним

        // Вероятность взять второй максимум
        if (next != default && Random.Shared.Next(_randBid) == 0)
            return next;
        else
            return max;
    }

    private bool IsConverged()
    {
        bool tempDepleted = _temperature <= _minTemperature;
        return tempDepleted || (_stableStateCount >= _stableLimit) || (_passes >= _maxRepeat);
    }

    private double CalculateTotalCoordinationCost()
    {
        var vertexCount = _dsm.DsmGraphView.Vertices.Count();
        var pairs = _dsm.DsmGraphView.Edges;
        double tcc = 0;

        foreach (var pair in _pairs.Keys)
        {
            tcc += IntraClusterCost(pair) + ExtraClusterCost(pair);
        }

        return tcc;
    }

    private double ExtraClusterCost(UnorderedPair<PSVertex> pair)
    {
        double cost = 0;
        int sourceCluster = Convert.ToInt32(pair.First.Metadata["cluster"]);
        int targetCluster = Convert.ToInt32(pair.Second.Metadata["cluster"]);

        if (sourceCluster != targetCluster)
            cost = _pairs[pair] * Math.Pow(_dsm.DsmGraphView.Vertices.Count(), _powCc);

        return cost;
    }

    private double IntraClusterCost(UnorderedPair<PSVertex> pair)
    {
        double cost = 0;
        int sourceCluster = Convert.ToInt32(pair.First.Metadata["cluster"]);
        int targetCluster = Convert.ToInt32(pair.Second.Metadata["cluster"]);

        if (sourceCluster == targetCluster)
            cost = _pairs[pair] * Math.Pow(_clusters[sourceCluster].Count, _powCc);

        return cost;
    }

    private PSVertex SelectRandomElement()
    {
        return _dsm.DsmGraphView.Vertices.ElementAt(Random.Shared.Next(_dsm.DsmGraphView.Vertices.Count()));
    }

    private IEnumerable<(int, double)> Bid(PSVertex element)
    {
        double bid = -1;
        foreach (var cluster in _clusters)
        {
            if (cluster.Value.Contains(element)) continue; // пропускаем кластер содержащий сам элемент.
            if (cluster.Value.Count >= _maxClusterSize) continue; // пропускаем кластеры с максимальным количеством элементов

            bid = 0;

            double inOut = 0;
            foreach (var item in cluster.Value)
            {
                inOut += _dsm[element, item] + _dsm[item, element];
            }
            int clusterSize = Math.Max(cluster.Value.Count, 1); // avoid div by zero
            bid = Math.Pow(inOut, _powDep) / Math.Pow(clusterSize, _powBid);
            yield return (cluster.Key, bid);
        }
        // If all clusters rejected (max size), allow no-op bid to enable random accept path
        if (bid == -1)
        {
            yield break; // no bids means element skipped silently
        }
    }

    // initialize partitions, one vertex per partition
    private void Initialize()
    {
        int i = 0;
        foreach (var v in _dsm.DsmGraphView.Vertices)
        {
            v.Metadata["cluster"] = i;
            _clusters.Add(i++, new List<PSVertex>() { v });
        }

        foreach (var pair in _dsm.DsmGraphView.Edges)
        {
            var idx = new UnorderedPair<PSVertex>(pair.Source, pair.Target);
            if (_pairs.ContainsKey(idx))
                _pairs[idx] += _dsm[pair.Source, pair.Target];
            else
                _pairs[idx] = _dsm[pair.Source, pair.Target];
        }

        _tcc = _bestTcc = CalculateTotalCoordinationCost();
        _costHistory.Add(_tcc); // record initial cost
        _bestDsm = _dsm.Clone();
    }

    private void InitializeTemperature()
    {
        _temperature = _initialTemperatureOverride ?? _tcc; // simple heuristic: start at cost scale
        if (_temperature <= 0) _temperature = 1.0; // guard
    }
}