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

    [Fact]
    public void SimAnneal_WithSeed_ShouldBeDeterministic()
    {
        var cfg = new DsmSimulatedAnnealingConfig
        {
            Times = 2,
            StableLimit = 2,
            MaxRepeat = 20,
            RandomSeed = 12345
        };

        var algo1 = new DsmSimulatedAnnealingAlgorithm(BuildSimpleDsm(), cfg);
        var ext1 = algo1.PartitionWithDetails();

        var algo2 = new DsmSimulatedAnnealingAlgorithm(BuildSimpleDsm(), cfg);
        var ext2 = algo2.PartitionWithDetails();

        ext1.BestCost.Should().BeApproximately(ext2.BestCost ?? 0, 1e-9);
        ext1.Passes.Should().Be(ext2.Passes);
        ext1.CostHistory.Should().Equal(ext2.CostHistory);
    }

    [Fact]
    public void SimAnneal_UsesInitialTemperatureOverride_WhenProvided()
    {
        var dsm = BuildSimpleDsm();
        var cfg = new DsmSimulatedAnnealingConfig
        {
            InitialTemperature = 42.0,
            MinTemperature = 1e-6,
            CoolingRate = 0.9,
            MaxRepeat = 2,
            StableLimit = 2
        };
        var algo = new DsmSimulatedAnnealingAlgorithm(dsm, cfg);

        var ext = algo.PartitionWithDetails();

        ext.TemperatureHistory.Should().NotBeNull();
        ext.TemperatureHistory!.Count.Should().BeGreaterThan(0);
        ext.TemperatureHistory[0].Should().BeApproximately(42.0, 1e-9);
    }

    [Fact]
    public void SimAnneal_StopReason_MaxRepeatReached_WhenPassLimitHits()
    {
        var dsm = BuildSimpleDsm();
        var cfg = new DsmSimulatedAnnealingConfig
        {
            MaxRepeat = 1,
            StableLimit = 100,
            MinTemperature = 0,
            CoolingRate = 1.0,
            RandomSeed = 7
        };
        var algo = new DsmSimulatedAnnealingAlgorithm(dsm, cfg);

        var ext = algo.PartitionWithDetails();

        ext.StopReason.Should().Be(AnnealingStopReason.MaxRepeatReached);
        ext.Passes.Should().Be(1);
    }

    [Fact]
    public void SimAnneal_StopReason_TemperatureDepleted_WhenInitialTemperatureBelowThreshold()
    {
        var dsm = BuildSimpleDsm();
        var cfg = new DsmSimulatedAnnealingConfig
        {
            InitialTemperature = 1e-6,
            MinTemperature = 1e-3,
            MaxRepeat = 10,
            StableLimit = 10
        };
        var algo = new DsmSimulatedAnnealingAlgorithm(dsm, cfg);

        var ext = algo.PartitionWithDetails();

        ext.StopReason.Should().Be(AnnealingStopReason.TemperatureDepleted);
        ext.Passes.Should().Be(0);
    }
}
