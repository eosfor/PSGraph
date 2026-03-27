using PSGraph.DesignStructureMatrix;

namespace PSGraph.Model;

public class PartitioningResult : IDsmPartitionResult
{
    public IDsm Dsm { get; set; }
    public IDsmPartitionAlgorithm Algorithm { get; set; }

    public IReadOnlyList<IReadOnlyList<PSVertex>> Partitions
    {
        get
        {
            if (Algorithm is null)
            {
                return Array.Empty<IReadOnlyList<PSVertex>>();
            }

            return Algorithm.Partitions
                .Select(static partition => (IReadOnlyList<PSVertex>)partition)
                .ToList();
        }
    }
}
