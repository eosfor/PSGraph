using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public class DsmClassicPartitioningAlgorithm: IDsmPartitionAlgorithm
{
    private DsmClassic _dsmObj;
    private IDsm _partitionedObj;
    private List<List<PSVertex>> _partitions = new();

    public IDsm Partitioned => _partitionedObj;
    public List<List<PSVertex>> Partitions => _partitions;


    public IDsm Partition()
    { var noOutput = FindTasksWithNoOutput(_dsmObj);
        var noInput = FindTasksWithNoInput(_dsmObj);

        var noEmptyLines = _dsmObj.Remove(noOutput);
        noEmptyLines = noEmptyLines.Remove(noInput);


        HashSet<List<PSVertex>> tempPart = new(new ListEqualityComparer());
        foreach (List<PSVertex> p in PartitionInternal(noEmptyLines))
        {
            p.Sort();
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

    private List<PSVertex> FindTasksWithNoOutput(DsmClassic dsmObj)
    {
        var ret = new List<PSVertex>();

        for (int i = 0; i < _dsmObj.DsmMatrixView.RowCount; i++)
        {
            if (_dsmObj.DsmMatrixView.Row(i).Sum() == 0)
            {
                ret.Add(dsmObj.RowIndex.Where(v => v.Value == i).First().Key);
            }
        }

        return ret;
    }

    private List<PSVertex> FindTasksWithNoInput(DsmClassic dsmObj)
    {
        var ret = new List<PSVertex>();

        for (int i = 0; i < _dsmObj.DsmMatrixView.ColumnCount; i++)
        {
            if (_dsmObj.DsmMatrixView.Column(i).Sum() == 0)
            {
                ret.Add(dsmObj.ColIndex.Where(v => v.Value == i).First().Key);
            }
        }

        return ret;
    }

    private IEnumerable<List<PSVertex>> PartitionInternal(IDsm dsmObj)
    {
        int power = 2;
        
        // raising matrix to a consecutive powers from 2 to matrix size -1
        for (int i = power; i <= dsmObj.DsmMatrixView.RowCount; i++)
        {
            var t = dsmObj.DsmMatrixView.Power(i);
            
            // if diagonal is non-zero -> there are loops
            if (t.Diagonal().Sum() > 0)
            {
                List<PSVertex> partition = new();
                
                // skimming through matrix to detect rows where diagonal element > 0
                for (int j = 0; j < dsmObj.DsmMatrixView.RowCount; j++)
                {
                    if (t.Diagonal()[j]> 0)
                    {
                        partition.Add(dsmObj.RowIndex.Where(v => v.Value == j).First().Key);
                    }
                }

                yield return partition;
            }

        }
    }

    public DsmClassicPartitioningAlgorithm(DsmClassic dsmObj)
    {
        _dsmObj = dsmObj;
    }
}