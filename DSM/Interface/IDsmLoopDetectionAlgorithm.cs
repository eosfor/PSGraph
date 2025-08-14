
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public interface IDsmLoopDetectionAlgorithm
{
    public List<List<PSVertex>> DetectLoops(IDsm dsm);
    public IDsm CondenceLoops(IDsm dsm, out List<List<PSVertex>> collapsedNodes);
}