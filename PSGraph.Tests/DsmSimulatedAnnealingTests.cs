using Xunit;
using FluentAssertions;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using System.Linq;

namespace PSGraph.Tests;

public class DsmSimulatedAnnealingTests
{
    private DsmClassic BuildSimpleDsm()
    {
        var g = new PsBidirectionalGraph();
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var c = new PSVertex("C");
        var d = new PSVertex("D");
        g.AddVertex(a); g.AddVertex(b); g.AddVertex(c); g.AddVertex(d);
        g.AddEdge(new PSEdge(a,b,new PSEdgeTag()));
        g.AddEdge(new PSEdge(b,c,new PSEdgeTag()));
        g.AddEdge(new PSEdge(c,a,new PSEdgeTag())); // cycle A-B-C
        g.AddEdge(new PSEdge(c,d,new PSEdgeTag())); // tail to D
        return new DsmClassic(g);
    }

    [Fact]
    public void SimAnneal_ShouldReturnPartitionedDsm()
    {
        var dsm = BuildSimpleDsm();
    var algo = new DsmSimulatedAnnealingAlgorithm(dsm, new DsmSimulatedAnnealingConfig() { Times = 1, StableLimit = 1, MaxRepeat = 50 } );
        var result = algo.Partition();
        result.Should().NotBeNull();
        algo.Partitions.Should().NotBeNull();
        algo.Partitions.SelectMany(p=>p).Distinct().Count().Should().Be(dsm.RowIndex.Count);
    }

    [Fact]
    public void SimAnneal_ShouldTrackCostHistory()
    {
        var dsm = BuildSimpleDsm();
    var algo = new DsmSimulatedAnnealingAlgorithm(dsm, new DsmSimulatedAnnealingConfig() { Times = 2, StableLimit = 2, MaxRepeat = 60 } );
        var result = algo.Partition();
        algo.CostHistory.Should().NotBeNull();
        algo.CostHistory.Count.Should().BeGreaterThanOrEqualTo(1); // at least initial cost
        algo.BestCost.Should().BeGreaterThan(0); // cost positive for this graph
    }

    [Fact]
    public void SimAnneal_OrderingApplied()
    {
        var dsm = BuildSimpleDsm();
    var algo = new DsmSimulatedAnnealingAlgorithm(dsm, new DsmSimulatedAnnealingConfig() { Times = 1, StableLimit = 1, MaxRepeat = 30 } );
        var result = algo.Partition();
        // Ensure that ordering produced a DSM containing all original vertices
        foreach(var v in dsm.RowIndex.Keys)
        {
            result.RowIndex.Should().ContainKey(v);
        }
    }
}
