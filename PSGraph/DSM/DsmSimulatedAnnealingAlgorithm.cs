using PSGraph.Model;

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

    // configuration (directly from original algo)
    private int _powCc = 1;     // penalty assigned to cluster size(2)
    private int _powBid = 1;		// high value penalizes large clusters(0-3, 2)
    private int _powDep = 2;		// high value emphasizes high interactions(0-2, 2)
    private int _maxClusterSize = 61;	// max size of cluster(DSM size)
    private int _randAccept = 122;	// proceed w/ 1 of N changes even if no imp. (0.5-2 *DSM)
    private int _randBid = 122;	// take second highest bid 1 out of N times(0.5-2 * DSM)
    private int _times = 2;		// attept time*size before check sys.stability(2)
    private int _stableLimit = 2;       // loop at least stable_limit*times* size(2)
    private int _maxRepeat = 10;

    double AbsEps = 1e-12;
    double RelEps = 1e-9;

    public DsmSimulatedAnnealingAlgorithm(IDsm dsm)
    {
        _dsm = dsm;
        _clusters = new Dictionary<int, List<PSVertex>>();
        _pairs = new();
        _improvementStats = new();

        _passes = 0;  //number of passes
        _stableStateCount = 0;
        _maxClusterSize = _dsm.DsmMatrixView.RowCount; // матрица квадратная, так что можно взять только количество строк
        _randAccept = _dsm.DsmMatrixView.RowCount * 2;
        _randBid = _dsm.DsmMatrixView.RowCount * 2;
    }



    public IDsm Partition()
    {
        Initialize();
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

                var bestBid = SelectBestBid(bidVectors); // в этот момент bids основаны на предположении что element уже в кластере?

                int oldClusterId = Convert.ToInt32(element.Metadata["cluster"]); // сохраняем старый кластер для отката

                var success = MoveVertex(bestBid.Item1, element);
                if (!success) continue;

                var newTcc = CalculateTotalCoordinationCost();

                var eps = Math.Max(AbsEps, RelEps * Math.Abs(_tcc));
                if (newTcc < _tcc - eps)
                    acceptedThisPass = true;
                else
                    acceptedThisPass = Random.Shared.Next(_randAccept) == 0; // 1 из N

                if (acceptedThisPass)
                {
                    _tcc = newTcc;

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
            }

            _passes++;
            if (!improvedThisPass) _stableStateCount++; else _stableStateCount = 0;

            int removedCount = RemoveEmptyClusters(_clusters);
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
        dsmToReturn.Order(newOrder);
        return dsmToReturn;
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
        return (_stableStateCount >= _stableLimit) || (_passes >= _maxRepeat);
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
            bid = (double)Math.Pow(inOut, _powDep) / (double)Math.Pow(cluster.Value.Count + 1, _powBid);
            yield return (cluster.Key, bid);
        }

        if (bid == -1) throw new InvalidDataException($"all clusters are at max size, so there is no space for element {element}");
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
        _bestDsm = _dsm.Clone();
    }
}