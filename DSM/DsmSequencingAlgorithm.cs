using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

public class DsmSequencingAlgorithm : IDsmSequencingAlgorithm
{
    private IDsm _dsm; //immutable, хранит ссылку на оригинальную DSM
    private IDsm _workDsm; //копия оригинальной для работы

    public DsmSequencingAlgorithm(IDsm dsm)
    {
        _dsm = dsm;
        _workDsm = _dsm.Clone();
    }
    public IDsm Sequence(IDsmLoopDetectionAlgorithm algorithm)
    {
        List<PSVertex> sources = new();
        List<PSVertex> sinks = new();
        List<List<PSVertex>> collapsedBlocks = new();

        while (true)
        {
            var so = _workDsm.GetSources();
            var si = _workDsm.GetSinks();
            var combinedDistict = so.Concat(si).Distinct().ToList();

            sources.AddRange(so);
            sinks.AddRange(si);

            if (IsConverged())
                break;

            // Удаляем источники и стоки из рабочей DSM
            if (combinedDistict.Count > 0)
            {
                _workDsm = _workDsm.Remove(combinedDistict);
            }

            _workDsm = algorithm.CondenceLoops(_workDsm, out List<List<PSVertex>> collapsed);
            if (collapsed?.Count > 0)
                collapsedBlocks.AddRange(collapsed);
        }

        // Итог: источники + развёрнутые блоки циклов + стоки
        var newOrder = sources
            .Distinct()
            .Concat(collapsedBlocks.SelectMany(b => b))
            .Concat(sinks.Distinct())
            .Distinct()
            .ToList();
    return _dsm.Order(newOrder);
    }

    private bool IsConverged()
    {
        return !_workDsm.RowIndex.Any();
    }
}