using PSGraph.Model;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.ConnectedComponents;

namespace PSGraph.DesignStructureMatrix;

public class GraphCondensationAlgorithm : IDsmLoopDetectionAlgorithm
{
    public IDsm CondenceLoops(IDsm dsm, out List<List<PSVertex>> collapsedBlocks)
    {
        List<List<PSVertex>> cb = new();
        // Конденсация графа  ─ CondensationGraphAlgorithm
        // Каждый super-vertex = HashSet<PSVertex> (одна SCC)
        var condAlg = new CondensationGraphAlgorithm<PSVertex, PSEdge, PsBidirectionalGraph>(dsm.DsmGraphView);
        condAlg.Compute();

        var sccBlocks = condAlg.CondensedGraph.Vertices
                                                .Select(scc => scc.Vertices.ToList())
                                                .Where(block => block.Count > 0)
                                                .ToList();

        if (sccBlocks.Count > 0)
            cb.AddRange(sccBlocks);

        collapsedBlocks = cb;
        return new DsmClassic(condAlg.CondensedGraph.ToPsBidirectionalGraph());
    }

    public List<List<PSVertex>> DetectLoops(IDsm dsm)
    {
        // Конденсация графа  ─ CondensationGraphAlgorithm
        // Каждый super-vertex = HashSet<PSVertex> (одна SCC)
        var sccAlg = new StronglyConnectedComponentsAlgorithm<PSVertex, PSEdge>(dsm.DsmGraphView);
        sccAlg.Compute();

        // Group vertices by component index
        var components = sccAlg.Components
            .GroupBy(kvp => kvp.Value)
            .Select(group => group.Select(kvp => kvp.Key).ToList())
            .Where(block => block.Count > 0)
            .ToList();

        return components;
    }
}