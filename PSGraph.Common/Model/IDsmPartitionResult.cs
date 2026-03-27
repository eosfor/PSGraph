using PSGraph.DesignStructureMatrix;

namespace PSGraph.Model;

public interface IDsmPartitionResult
{
    public IDsm Dsm { get; }
    public IReadOnlyList<IReadOnlyList<PSVertex>> Partitions { get; }
}