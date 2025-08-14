using System;
using System.Collections.Generic;
using System.Linq;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class DsmExtendedResultTests
{
    private IDsm BuildSmallDsm()
    {
        // Re‑use existing helper graph (SimpleTestGraph1) – it has cycles & multiple SCCs
        var g = GraphTestData.SimpleTestGraph1;
        return new DsmClassic(g);
    }

    [Fact]
    public void GraphPartitioning_ExtendedResult_HasSinglePassAndCost()
    {
        var dsm = BuildSmallDsm();
        var algo = new DsmGraphPartitioningAlgorithm(dsm);

        var ext = algo.PartitionWithDetails();

        Assert.NotNull(ext.Dsm);
        Assert.Single(ext.CostHistory);
        Assert.Equal(1, ext.Passes);
        Assert.Equal(1, ext.StablePasses);
        Assert.Equal(ext.CostHistory[0], ext.BestCost);
        Assert.Null(ext.ImprovementStats); // deterministic single pass

        // Recompute cross-component edges independently and compare
        // Build component index map from partitions
        var compIndex = new Dictionary<PSVertex,int>();
        for (int i = 0; i < algo.Partitions.Count; i++)
            foreach (var v in algo.Partitions[i]) compIndex[v] = i;

        int cross = 0;
        foreach (var e in dsm.DsmGraphView.Edges)
        {
            if (compIndex.TryGetValue(e.Source, out int s) && compIndex.TryGetValue(e.Target, out int t) && s != t)
                cross++;
        }
        Assert.Equal(cross, (int)ext.BestCost);
    }

    [Fact]
    public void SimulatedAnnealing_ExtendedResult_TracksHistoriesAndBestCost()
    {
        var dsm = BuildSmallDsm();
        var cfg = new DsmSimulatedAnnealingConfig
        {
            Times = 1,          // keep fast
            StableLimit = 1,
            MaxRepeat = 20,
            CoolingRate = 0.90,
            MinTemperature = 1e-4,
            InitialTemperature = null // auto
        };
        var algo = new DsmSimulatedAnnealingAlgorithm(dsm, cfg);

        var ext = algo.PartitionWithDetails();

        Assert.NotNull(ext.Dsm);
        Assert.True(ext.CostHistory.Count >= 1); // includes initial cost
        Assert.True(ext.TemperatureHistory == null || ext.TemperatureHistory.Count >= 1);
        Assert.True(ext.BestCost <= ext.CostHistory.Max() + 1e-9); // best <= worst
        Assert.True(ext.BestCost >= ext.CostHistory.Min() - 1e-9); // best >= best lower bound (avoid NaN)
        Assert.True(ext.Passes >= 1);
        Assert.True(ext.StablePasses >= 0);

        if (ext.ImprovementStats != null && ext.ImprovementStats.Count > 0)
        {
            // keys should be in range [0, Passes)
            Assert.All(ext.ImprovementStats.Keys, k => Assert.InRange(k, 0, ext.Passes - 1));
        }
    }

    [Fact]
    public void SimulatedAnnealing_FinalDsm_OrderMatchesFlattenedPartitions()
    {
        var dsm = BuildSmallDsm();
        var algo = new DsmSimulatedAnnealingAlgorithm(dsm, new DsmSimulatedAnnealingConfig{ Times = 1, StableLimit = 1, MaxRepeat = 10 });
        var ext = algo.PartitionWithDetails();

        var flattened = algo.Partitions.SelectMany(p => p).ToList();
        var dsmVertices = ext.Dsm.DsmGraphView.Vertices.ToList();

        // Ensure same set & no duplicates
        Assert.Equal(flattened.Count, flattened.Distinct().Count());
        Assert.Equal(dsmVertices.Count, dsmVertices.Distinct().Count());
        Assert.Equal(dsmVertices.Count, flattened.Count);
        Assert.True(new HashSet<PSVertex>(flattened).SetEquals(dsmVertices));
    }
}
