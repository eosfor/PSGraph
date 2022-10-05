using PSGraph.DesignStructureMatrix;

namespace PSGraph.Model;

public class PartitioningResult
{
    public IDsm Dsm { get; set; }
    public IDsmPartitionAlgorithm Algorithm { get; set; }
}
