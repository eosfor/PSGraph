using PSGraph.Model;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.TopologicalSort;

namespace PSGraph.DesignStructureMatrix;

public class DsmGraphPartitioningAlgorithm : IDsmPartitionAlgorithm
{
    private IDsm _dsmObj;
    private IDsm _partitionedObj;
    private List<List<PSVertex>> _partitions = new();
    public IDsm Partitioned => _partitionedObj;
    public List<List<PSVertex>> Partitions => _partitions;

    public IDsm Partition()
    {
        // 1) источники / стоки
        var sinks = FindTasksWithNoOutput(_dsmObj);
        var sources = FindTasksWithNoInput(_dsmObj);
        // var toRemove = sinks.Concat(sources).Distinct().ToList();
        // var trimmed = _dsmObj.Remove(toRemove);


        // 2) SCC → топологический порядок (уже DAG)
        var sccBlocks = GetTopoOrderedScc(_dsmObj).ToList();

        // 3) итоговый порядок строк/столбцов
        var newOrder = sources
                       .Concat(sccBlocks.SelectMany(b => b))
                       .Concat(sinks)
                       .Distinct()
                       .ToList();

        // переставляем матрицу
        _partitionedObj = _dsmObj.Order(newOrder); 
        _partitions = sccBlocks;
        return _partitionedObj;
    }


    private List<PSVertex> FindTasksWithNoOutput(IDsm dsmObj)
    {
        return dsmObj.DsmGraphView.Vertices.Where(v => _dsmObj.DsmGraphView.OutDegree(v) == 0).Distinct().ToList();
    }

    private List<PSVertex> FindTasksWithNoInput(IDsm dsmObj)
    {
        return dsmObj.DsmGraphView.Vertices.Where(v => _dsmObj.DsmGraphView.InDegree(v) == 0).Distinct().ToList();
    }

    private IEnumerable<List<PSVertex>> PartitionInternal(IDsm dsmObj)
    {
        var algo = new StronglyConnectedComponentsAlgorithm<PSVertex, PSEdge>(dsmObj.DsmGraphView);
        algo.Compute();

        var groups = algo.Components.GroupBy(v => v.Value).Select(v => v);

        foreach (var grp in groups)
        {
            yield return grp.Select(p => p.Key).ToList();
        }
    }

    public DsmGraphPartitioningAlgorithm(IDsm dsmObj)
    {
        _dsmObj = dsmObj;
    }

    private static IEnumerable<List<PSVertex>> GetTopoOrderedScc(IDsm dsm)
    {
        // Исходный граф зависимостей
        var graph = dsm.DsmGraphView;
        // заполняет sccAlg.Components  (Dictionary<vertex,int>)

        // Конденсация графа  ─ CondensationGraphAlgorithm
        // Каждый super-vertex = HashSet<PSVertex> (одна SCC)
        var condAlg = new CondensationGraphAlgorithm<PSVertex, PSEdge, BidirectionalGraph<PSVertex, PSEdge>>(graph);
        condAlg.Compute();

        var condGraph = condAlg.CondensedGraph;   // BidirectionalGraph<HashSet<PSVertex>, SEdge<HashSet<PSVertex>>>

        var topoAlg = new TopologicalSortAlgorithm<
            BidirectionalGraph<PSVertex, PSEdge>,      // вершины DAG
            CondensedEdge<PSVertex, PSEdge,
                          BidirectionalGraph<PSVertex, PSEdge>> // рёбра DAG
            >(condGraph);

        topoAlg.Compute();                         // заполняет SortedVertices

        // Разворачиваем компоненты в найденном порядке
        foreach (var comp in topoAlg.SortedVertices)  // comp = BidirectionalGraph<PSVertex,PSEdge>
            yield return comp.Vertices.ToList();      // сохраним внешний API: List<PSVertex>
    }

}