using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSGraph.DesignStructureMatrix;

public class DsmSimulatedAnnealingAlgorithm : IDsmPartitionAlgorithm
{
    private IDsm _dsm;
    private IDsm _bestDsm;
    private IDictionary<int, List<PSVertex>> _bestClusterLayout;

    private IDictionary<int, List<PSVertex>> _clusters;
    private Dictionary<UnorderedPair<PSVertex>, double> _pairs;
    private Dictionary<int, int> _improvementStats;

    private int _passes;
    private int _stableStateCount;

    private double _currentCost;
    private double _bestCost;

    public IDsm Partitioned => _dsm;
    public Dictionary<int, int> ImprovementStats => _improvementStats;
    public List<List<PSVertex>> Partitions => _clusters.Values.ToList();

    // Legacy cost model parameters
    private int _powCc = 1;
    private int _powBid = 0;
    private int _powDep = 0;
    private int _maxClusterSize = 0;
    private int _randBid = 2;
    private int _times = 2;
    private int _stableLimit = 2;
    private int _maxRepeat = 1000;

    // Simulated annealing parameters
    private double _temperature;
    private double _coolingRate = 0.95;
    private double _minTemperature = 1e-3;
    private double? _initialTemperatureOverride;
    private int _epochLength = 0;
    private AnnealingCoolingSchedule _coolingSchedule = AnnealingCoolingSchedule.Geometric;
    private double _initialAcceptanceProbability = 0.8;
    private int _temperatureCalibrationMoves = 32;
    private int? _randomSeed;

    // Run metrics
    private int _acceptedMoves;
    private int _rejectedMoves;
    private int _acceptedWorseMoves;
    private AnnealingStopReason _stopReason;

    private readonly List<double> _costHistory = new();
    public IReadOnlyList<double> CostHistory => _costHistory;

    private readonly List<double> _temperatureHistory = new();
    public IReadOnlyList<double> TemperatureHistory => _temperatureHistory;

    public double BestCost => _bestCost;
    public AnnealingStopReason StopReason => _stopReason;

    private readonly Random _random;

    private const double AbsEps = 1e-12;
    private const double RelEps = 1e-9;

    public DsmSimulatedAnnealingAlgorithm(IDsm dsm, DsmSimulatedAnnealingConfig? cfg = null)
    {
        _dsm = _bestDsm = dsm;
        _clusters = new Dictionary<int, List<PSVertex>>();
        _bestClusterLayout = new Dictionary<int, List<PSVertex>>();
        _pairs = new Dictionary<UnorderedPair<PSVertex>, double>();
        _improvementStats = new Dictionary<int, int>();

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
            _epochLength = cfg.EpochLength;
            _coolingSchedule = cfg.CoolingSchedule;
            _initialAcceptanceProbability = cfg.InitialAcceptanceProbability;
            _temperatureCalibrationMoves = cfg.TemperatureCalibrationMoves;
            _randomSeed = cfg.RandomSeed;
        }

        ValidateConfiguration();

        _maxClusterSize = _dsm.DsmMatrixView.RowCount;
        _randBid = Math.Max(2, _dsm.DsmMatrixView.RowCount * 2);
        _random = _randomSeed.HasValue ? new Random(_randomSeed.Value) : Random.Shared;
    }

    public IDsm Partition()
    {
        ResetRunState();
        Initialize();
        InitializeTemperature();

        AnnealingStopReason stopReason = AnnealingStopReason.None;
        while (!IsConverged(out stopReason))
        {
            _temperatureHistory.Add(_temperature);

            bool improvedThisPass = false;
            _improvementStats[_passes] = 0;

            int epochMoves = ResolveEpochLength();
            for (int i = 0; i < epochMoves; i++)
            {
                if (!TryEvaluateMove(out var element, out var oldClusterId, out var newCost, out var delta))
                {
                    continue;
                }

                bool acceptedWorseMove;
                bool accepted = ShouldAcceptMove(newCost, delta, out acceptedWorseMove);
                if (accepted)
                {
                    _acceptedMoves++;
                    if (acceptedWorseMove)
                    {
                        _acceptedWorseMoves++;
                    }

                    _currentCost = newCost;
                    _costHistory.Add(_currentCost);

                    if (IsStrictlyLess(_currentCost, _bestCost))
                    {
                        _bestDsm = _dsm.Clone();
                        _bestClusterLayout = DeepCopyClusterLayout(_clusters);
                        _bestCost = _currentCost;
                        improvedThisPass = true;
                        _improvementStats[_passes]++;
                    }
                }
                else
                {
                    _rejectedMoves++;
                    MoveVertex(oldClusterId, element);
                }
            }

            _passes++;
            _stableStateCount = improvedThisPass ? 0 : _stableStateCount + 1;

            RemoveEmptyClusters(_clusters);
            if (_costHistory.Count == 0 || !AreAlmostEqual(_costHistory[^1], _currentCost))
            {
                _costHistory.Add(_currentCost);
            }

            _temperature = CoolTemperature(_temperature, _passes);
        }

        _stopReason = stopReason;

        IDsm dsmToReturn;
        IDictionary<int, List<PSVertex>> clusterLayoutToReturn;
        if (IsStrictlyLess(_bestCost, _currentCost))
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
        dsmToReturn = (IDsm)dsmToReturn.Order(newOrder);

        _dsm = dsmToReturn;
        _clusters = DeepCopyClusterLayout(clusterLayoutToReturn);

        return _dsm;
    }

    public PartitioningExtendedResult PartitionWithDetails()
    {
        var dsm = Partition();
        int totalAttempts = _acceptedMoves + _rejectedMoves;
        double acceptanceRate = totalAttempts == 0 ? 0.0 : (double)_acceptedMoves / totalAttempts;

        return new PartitioningExtendedResult
        {
            Dsm = dsm,
            Algorithm = this,
            CostHistory = CostHistory,
            TemperatureHistory = TemperatureHistory,
            ImprovementStats = new Dictionary<int, int>(ImprovementStats),
            BestCost = BestCost,
            Passes = _passes,
            StablePasses = _stableStateCount,
            StopReason = _stopReason,
            AcceptedMoves = _acceptedMoves,
            RejectedMoves = _rejectedMoves,
            AcceptedWorseMoves = _acceptedWorseMoves,
            AcceptanceRate = acceptanceRate
        };
    }

    private void ResetRunState()
    {
        _clusters = new Dictionary<int, List<PSVertex>>();
        _bestClusterLayout = new Dictionary<int, List<PSVertex>>();
        _pairs = new Dictionary<UnorderedPair<PSVertex>, double>();
        _improvementStats = new Dictionary<int, int>();

        _passes = 0;
        _stableStateCount = 0;

        _acceptedMoves = 0;
        _rejectedMoves = 0;
        _acceptedWorseMoves = 0;
        _stopReason = AnnealingStopReason.None;

        _costHistory.Clear();
        _temperatureHistory.Clear();

        _bestDsm = _dsm;
    }

    private void ValidateConfiguration()
    {
        _times = Math.Max(1, _times);
        _stableLimit = Math.Max(1, _stableLimit);
        _maxRepeat = Math.Max(1, _maxRepeat);
        _epochLength = Math.Max(0, _epochLength);
        _temperatureCalibrationMoves = Math.Max(1, _temperatureCalibrationMoves);

        if (!double.IsFinite(_initialAcceptanceProbability))
        {
            _initialAcceptanceProbability = 0.8;
        }
        _initialAcceptanceProbability = Math.Clamp(_initialAcceptanceProbability, 0.01, 0.99);

        if (!double.IsFinite(_coolingRate) || _coolingRate <= 0)
        {
            _coolingRate = 0.95;
        }

        if (_coolingSchedule == AnnealingCoolingSchedule.Geometric && _coolingRate >= 1.0)
        {
            _coolingRate = 0.95;
        }

        if (!double.IsFinite(_minTemperature) || _minTemperature < 0)
        {
            _minTemperature = 1e-3;
        }

        if (_initialTemperatureOverride.HasValue)
        {
            double t0 = _initialTemperatureOverride.Value;
            if (!double.IsFinite(t0) || t0 <= 0)
            {
                _initialTemperatureOverride = null;
            }
        }
    }

    private int ResolveEpochLength()
    {
        if (_epochLength > 0)
        {
            return _epochLength;
        }

        return Math.Max(1, _times * Math.Max(_maxClusterSize, 1));
    }

    private bool IsConverged(out AnnealingStopReason reason)
    {
        if (_maxClusterSize == 0)
        {
            reason = AnnealingStopReason.EmptyGraph;
            return true;
        }

        if (_passes >= _maxRepeat)
        {
            reason = AnnealingStopReason.MaxRepeatReached;
            return true;
        }

        if (_stableStateCount >= _stableLimit)
        {
            reason = AnnealingStopReason.StableLimitReached;
            return true;
        }

        if (_temperature <= _minTemperature)
        {
            reason = AnnealingStopReason.TemperatureDepleted;
            return true;
        }

        reason = AnnealingStopReason.None;
        return false;
    }

    private bool TryEvaluateMove(out PSVertex element, out int oldClusterId, out double newCost, out double delta)
    {
        element = null!;
        oldClusterId = -1;
        newCost = _currentCost;
        delta = 0;

        if (_maxClusterSize <= 1)
        {
            return false;
        }

        element = SelectRandomElement();
        var bidVectors = Bid(element).ToList();
        if (bidVectors.Count == 0)
        {
            return false;
        }

        var selectedBid = SelectBestBid(bidVectors);
        oldClusterId = Convert.ToInt32(element.Metadata["cluster"]);

        if (!MoveVertex(selectedBid.Item1, element))
        {
            return false;
        }

        newCost = CalculateTotalCoordinationCost();
        delta = newCost - _currentCost;
        return true;
    }

    private bool ShouldAcceptMove(double newCost, double delta, out bool acceptedWorseMove)
    {
        acceptedWorseMove = false;

        if (IsStrictlyLess(newCost, _currentCost))
        {
            return true;
        }

        if (delta <= 0)
        {
            return true;
        }

        if (_temperature <= double.Epsilon)
        {
            return false;
        }

        double exponent = -delta / _temperature;
        exponent = Math.Clamp(exponent, -700, 0);

        double probability = Math.Exp(exponent);
        bool accepted = _random.NextDouble() < probability;
        acceptedWorseMove = accepted;
        return accepted;
    }

    private double CoolTemperature(double currentTemperature, int epochIndex)
    {
        return _coolingSchedule switch
        {
            AnnealingCoolingSchedule.Geometric => currentTemperature * _coolingRate,
            AnnealingCoolingSchedule.Linear => Math.Max(0.0, currentTemperature - _coolingRate),
            AnnealingCoolingSchedule.Logarithmic => currentTemperature / (1.0 + _coolingRate * Math.Log(epochIndex + 1.0)),
            _ => currentTemperature * _coolingRate
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
        if (!element.Metadata.ContainsKey("cluster"))
        {
            throw new InvalidDataException($"cluster node {element} does not have metadata[\"cluster\"] property set");
        }

        int sourceClusterId = Convert.ToInt32(element.Metadata["cluster"]);

        if (sourceClusterId == targetClusterId)
        {
            return false;
        }

        if (_clusters[sourceClusterId].Remove(element))
        {
            _clusters[targetClusterId].Add(element);
            element.Metadata["cluster"] = targetClusterId;
            return true;
        }

        return false;
    }

    private (int, double) SelectBestBid(IEnumerable<(int, double)> bidVectors)
    {
        var top2 = bidVectors
            .OrderByDescending(x => x.Item2)
            .Take(2)
            .ToList();

        var max = top2.ElementAtOrDefault(0);
        var next = top2.ElementAtOrDefault(1);

        if (next != default && _random.Next(_randBid) == 0)
        {
            return next;
        }

        return max;
    }

    private double CalculateTotalCoordinationCost()
    {
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
        {
            cost = _pairs[pair] * Math.Pow(_dsm.DsmGraphView.Vertices.Count(), _powCc);
        }

        return cost;
    }

    private double IntraClusterCost(UnorderedPair<PSVertex> pair)
    {
        double cost = 0;
        int sourceCluster = Convert.ToInt32(pair.First.Metadata["cluster"]);
        int targetCluster = Convert.ToInt32(pair.Second.Metadata["cluster"]);

        if (sourceCluster == targetCluster)
        {
            cost = _pairs[pair] * Math.Pow(_clusters[sourceCluster].Count, _powCc);
        }

        return cost;
    }

    private PSVertex SelectRandomElement()
    {
        var vertices = _dsm.DsmGraphView.Vertices;
        return vertices.ElementAt(_random.Next(vertices.Count()));
    }

    private IEnumerable<(int, double)> Bid(PSVertex element)
    {
        bool yielded = false;

        foreach (var cluster in _clusters)
        {
            if (cluster.Value.Contains(element))
            {
                continue;
            }

            if (cluster.Value.Count >= _maxClusterSize)
            {
                continue;
            }

            double inOut = 0;
            foreach (var item in cluster.Value)
            {
                inOut += _dsm[element, item] + _dsm[item, element];
            }

            int clusterSize = Math.Max(cluster.Value.Count, 1);
            double bid = Math.Pow(inOut, _powDep) / Math.Pow(clusterSize, _powBid);
            yielded = true;
            yield return (cluster.Key, bid);
        }

        if (!yielded)
        {
            yield break;
        }
    }

    private void Initialize()
    {
        int i = 0;
        foreach (var v in _dsm.DsmGraphView.Vertices)
        {
            v.Metadata["cluster"] = i;
            _clusters.Add(i++, new List<PSVertex> { v });
        }

        foreach (var pair in _dsm.DsmGraphView.Edges)
        {
            var idx = new UnorderedPair<PSVertex>(pair.Source, pair.Target);
            if (_pairs.ContainsKey(idx))
            {
                _pairs[idx] += _dsm[pair.Source, pair.Target];
            }
            else
            {
                _pairs[idx] = _dsm[pair.Source, pair.Target];
            }
        }

        _currentCost = _bestCost = CalculateTotalCoordinationCost();
        _costHistory.Add(_currentCost);

        _bestDsm = _dsm.Clone();
        _bestClusterLayout = DeepCopyClusterLayout(_clusters);
    }

    private void InitializeTemperature()
    {
        if (_initialTemperatureOverride.HasValue)
        {
            _temperature = _initialTemperatureOverride.Value;
        }
        else
        {
            _temperature = CalibrateInitialTemperature();
        }

        if (!double.IsFinite(_temperature) || _temperature <= 0)
        {
            _temperature = Math.Max(_currentCost, 1.0);
        }
    }

    private double CalibrateInitialTemperature()
    {
        if (_maxClusterSize <= 1)
        {
            return Math.Max(_currentCost, 1.0);
        }

        var positiveDeltas = new List<double>(_temperatureCalibrationMoves);

        for (int i = 0; i < _temperatureCalibrationMoves; i++)
        {
            if (!TryEvaluateMove(out var element, out var oldClusterId, out _, out var delta))
            {
                continue;
            }

            MoveVertex(oldClusterId, element);

            if (delta > 0)
            {
                positiveDeltas.Add(delta);
            }
        }

        if (positiveDeltas.Count == 0)
        {
            return Math.Max(_currentCost, 1.0);
        }

        double avgDelta = positiveDeltas.Average();
        double probability = Math.Clamp(_initialAcceptanceProbability, 0.01, 0.99);
        double t0 = -avgDelta / Math.Log(probability);

        if (!double.IsFinite(t0) || t0 <= 0)
        {
            return Math.Max(_currentCost, 1.0);
        }

        return t0;
    }

    private static IDictionary<int, List<PSVertex>> DeepCopyClusterLayout(IDictionary<int, List<PSVertex>> source)
    {
        return source.ToDictionary(
            kvp => kvp.Key,
            kvp => new List<PSVertex>(kvp.Value));
    }

    private static bool AreAlmostEqual(double left, double right)
    {
        double eps = Math.Max(AbsEps, RelEps * Math.Max(Math.Abs(left), Math.Abs(right)));
        return Math.Abs(left - right) <= eps;
    }

    private static bool IsStrictlyLess(double left, double right)
    {
        double eps = Math.Max(AbsEps, RelEps * Math.Abs(right));
        return left < right - eps;
    }
}
