using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public class DsmPowersLoopDetectionAlgorithm : IDsmLoopDetectionAlgorithm
{

    private IDsm _dsmObj;

    public DsmPowersLoopDetectionAlgorithm(IDsm dsmObj)
    {
        _dsmObj = dsmObj;
    }

    // Реализация интерфейса: возвращает агрегированные циклы и новый DSM с удалёнными вершинами без входов/выходов
    public IDsm CondenceLoops(IDsm dsm, out List<List<PSVertex>> collapsedBlocks)
    {
        var blocks = DetectLoops(dsm);
        collapsedBlocks = blocks;
        // Для совместимости: возвращаем DSM без вершин без входов/выходов
        var noOutput = dsm.GetSinks();
        var noInput = dsm.GetSources();
        var toRemove = noOutput.Union(noInput).Distinct().ToList();
        var newDsm = dsm.Remove(toRemove);
        return newDsm;
    }

    public List<List<PSVertex>> DetectLoops(IDsm dsmObj)
    {
        var noOutput = dsmObj.GetSinks();
        var noInput = dsmObj.GetSources();
        var toRemove = noOutput.Union(noInput).Distinct().ToList();
        var noEmptyLines = dsmObj.Remove(toRemove);

        HashSet<List<PSVertex>> tempPart = new(new ListEqualityComparer());
        foreach (List<PSVertex> p in PartitionInternal(noEmptyLines))
        {
            p.Sort();
            tempPart.Add(p);
        }
        var partitions = tempPart.OrderByDescending(v => v.Count).ToList();
        return partitions;
    }

    private IEnumerable<List<PSVertex>> PartitionInternal(IDsm dsmObj)
    {
        int power = 2;
        for (int i = power; i <= dsmObj.DsmMatrixView.RowCount; i++)
        {
            var t = dsmObj.DsmMatrixView.Power(i);
            if (t.Diagonal().Sum() > 0)
            {
                List<PSVertex> partition = new();
                for (int j = 0; j < dsmObj.DsmMatrixView.RowCount; j++)
                {
                    if (t.Diagonal()[j] > 0)
                    {
                        partition.Add(dsmObj.RowIndex.Where(v => v.Value == j).First().Key);
                    }
                }
                yield return partition;
            }
        }
    }
}