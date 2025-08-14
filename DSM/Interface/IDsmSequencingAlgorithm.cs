
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public interface IDsmSequencingAlgorithm
{
    public IDsm Sequence(IDsmLoopDetectionAlgorithm algorithm);
}