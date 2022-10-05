using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public interface IDsmPartitionAlgorithm
{
    public IDsm Partitioned { get;  }
    public List<List<PSVertex>> Partitions { get;  }
    public IDsm Partition();
}