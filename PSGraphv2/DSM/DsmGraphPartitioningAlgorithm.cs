using PSGraph.Model;
using QuikGraph.Algorithms.ConnectedComponents;

namespace PSGraph.DesignStructureMatrix;

public class DsmGraphPartitioningAlgorithm: IDsmPartitionAlgorithm
{
    private IDsm _dsmObj;
    private IDsm _partitionedObj;
    private List<List<PSVertex>> _partitions = new();
    public IDsm Partitioned => _partitionedObj;
    public List<List<PSVertex>> Partitions => _partitions;
    
    
    public IDsm Partition()
    {
        var noOutput = FindTasksWithNoOutput(_dsmObj);
        var noInput = FindTasksWithNoInput(_dsmObj);

        var noEmptyLines = _dsmObj.Remove(noOutput);
        noEmptyLines = noEmptyLines.Remove(noInput);


        HashSet<List<PSVertex>> tempPart = new(new ListEqualityComparer());
        foreach (List<PSVertex> p in PartitionInternal(noEmptyLines))
        {
            p.Sort((x, y) => _dsmObj.DsmGraphView.Degree(x) - _dsmObj.DsmGraphView.Degree(y));
            tempPart.Add(p);
        }
        _partitions = tempPart.OrderByDescending(v => v.Count).ToList();


        List<PSVertex> order = new();
        order = order.Concat(noOutput).ToList();
        foreach (var p in _partitions)
        {
            order = order.Concat(p).ToList();
        }
        order = order.Concat(noInput).ToList();
        
        _partitionedObj = (IDsm)_dsmObj.Order(order);

        return Partitioned;
    }

    private List<PSVertex> FindTasksWithNoOutput(IDsm dsmObj)
    {
        return _dsmObj.DsmGraphView.Vertices.Where(v => _dsmObj.DsmGraphView.OutDegree(v) == 0).ToList();
    }
    
    private List<PSVertex> FindTasksWithNoInput(IDsm dsmObj)
    {
        return _dsmObj.DsmGraphView.Vertices.Where(v => _dsmObj.DsmGraphView.InDegree(v) == 0).ToList();
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
    
    public DsmGraphPartitioningAlgorithm(DsmClassic dsmObj)
    {
        _dsmObj = dsmObj;
    }
}