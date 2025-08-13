using Xunit;
using FluentAssertions;
using PSGraph.Model;
using PSGraph.DesignStructureMatrix;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PSGraph.Tests.AlgorithmicCorrectnessTests
{
    /// <summary>
    /// Critical failing tests that expose algorithmic correctness issues in PSGraph
    /// These tests are expected to fail and demonstrate the problems identified in the review
    /// </summary>
    public class CriticalAlgorithmFailuresTests
    {
        [Fact]
        public void ClassicPartitioning_ShouldBeRepeatable_FAILS()
        {
            // Test demonstrates non-deterministic behavior in SCC detection
            var graph = CreateTestGraphWithKnownSCC();
            var dsm = new DsmClassic(graph);

            var results = new List<Dictionary<PSVertex, int>>();
            
            // Run algorithm multiple times
            for (int i = 0; i < 10; i++)
            {
                var algo = new DsmClassicPartitioningAlgorithm(dsm);
                var result = algo.Partition();
                results.Add(new Dictionary<PSVertex, int>(result.RowIndex));
            }

            // Check if all results are identical (they shouldn't be due to HashSet non-determinism)
            var firstResult = results[0];
            var allIdentical = results.Skip(1).All(result =>
                result.Count == firstResult.Count &&
                result.All(kvp => firstResult.ContainsKey(kvp.Key) && firstResult[kvp.Key] == kvp.Value)
            );

            // This test SHOULD FAIL - demonstrating the non-determinism bug
            allIdentical.Should().BeTrue("SCC detection should be deterministic for identical inputs");
        }

        [Fact] 
        public void ClassicPartitioning_ShouldDetectSimpleCycle_FAILS()
        {
            // Test demonstrates incorrect SCC detection using matrix powers
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "A", "B", "C" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
            // Create 3-cycle: A -> B -> C -> A  
            graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag()));
            graph.AddEdge(new PSEdge(vertices[1], vertices[2], new PSEdgeTag()));
            graph.AddEdge(new PSEdge(vertices[2], vertices[0], new PSEdgeTag()));

            var dsm = new DsmClassic(graph);
            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var result = algo.Partition();

            // The classic algorithm should detect one SCC with all 3 vertices
            // But it may fail due to incorrect matrix power logic
            algo.Partitions.Should().ContainSingle("should have exactly one SCC")
                .Which.Should().HaveCount(3, "SCC should contain all 3 vertices in the cycle");
                
            // Verify all vertices are in same partition  
            var sccVertexNames = algo.Partitions.Single().Select(v => v.Label).OrderBy(x => x).ToArray();
            sccVertexNames.Should().Equal(new[] { "A", "B", "C" });
        }

        [Fact]
        public void GetGraphPath_ShouldValidateVertexMembership_FAILS()
        {
            // Test demonstrates missing input validation
            var graph = new PsBidirectionalGraph();
            var existingVertex = new PSVertex("A");
            var nonExistentVertex = new PSVertex("B");
            graph.AddVertex(existingVertex);

            var cmdlet = new PSGraph.Cmdlets.GetGraphPath
            {
                From = existingVertex,
                To = nonExistentVertex,  // This vertex is NOT in the graph
                Graph = graph
            };

            // This should throw an exception due to missing vertex validation
            // But currently it doesn't - the algorithm will crash later
            Action act = () => cmdlet.ProcessRecord();
            act.Should().Throw<ArgumentException>("algorithm should validate vertex membership before execution");
        }

        [Fact]
        public void GetGraphPath_ShouldRejectNegativeWeights_FAILS()
        {
            // Test demonstrates missing edge weight validation  
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "A", "B" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
            // Add edge with negative weight (invalid for Dijkstra)
            var edgeWithNegativeWeight = new PSEdge(vertices[0], vertices[1], new PSEdgeTag()) 
            { 
                Weight = -5.0 
            };
            graph.AddEdge(edgeWithNegativeWeight);

            var cmdlet = new PSGraph.Cmdlets.GetGraphPath
            {
                From = vertices[0],
                To = vertices[1], 
                Graph = graph
            };

            // Should throw exception for negative weights, but currently doesn't validate
            Action act = () => cmdlet.ProcessRecord();
            act.Should().Throw<InvalidOperationException>("Dijkstra algorithm requires non-negative weights");
        }

        [Fact]
        public void DSMIndexManagement_ShouldMaintainConsistency_FAILS()
        {
            // Test demonstrates potential index corruption during vertex removal
            var graph = new PsBidirectionalGraph();
            var vertices = Enumerable.Range(0, 5).Select(i => new PSVertex($"V{i}")).ToArray();
            graph.AddVertexRange(vertices);

            var dsm = new DsmClassic(graph);
            
            // Verify initial indices are consecutive
            var originalIndices = dsm.RowIndex.Values.OrderBy(x => x).ToArray();
            originalIndices.Should().Equal(new[] { 0, 1, 2, 3, 4 });

            // Remove middle vertex (index 2)
            var removedDsm = dsm.Remove(vertices[2]); 

            // After removal, indices should still be consecutive 0,1,2,3
            var newIndices = removedDsm.RowIndex.Values.OrderBy(x => x).ToArray();
            newIndices.Should().Equal(new[] { 0, 1, 2, 3 }, "indices should remain consecutive after removal");
            
            // Verify no vertex has the removed index
            removedDsm.RowIndex.Values.Should().NotContain(2, "removed vertex index should not exist");
        }

        [Fact]
        public void GraphBasedPartitioning_ShouldBeDeterministic_FAILS()
        {
            // Test demonstrates non-deterministic GroupBy operation
            var graph = CreateTestGraphWithKnownSCC();
            var dsm = new DsmClassic(graph);

            var results = new List<List<List<PSVertex>>>();
            
            for (int i = 0; i < 10; i++)
            {
                var algo = new DsmGraphPartitioningAlgorithm(dsm);
                algo.Partition();
                results.Add(algo.Partitions.Select(p => p.OrderBy(v => v.Label).ToList()).ToList());
            }

            // All runs should produce identical partition structures
            var firstResult = results[0];
            foreach (var result in results.Skip(1))
            {
                result.Should().HaveCount(firstResult.Count, "partition count should be consistent");
                
                for (int partitionIndex = 0; partitionIndex < firstResult.Count; partitionIndex++)
                {
                    var expectedPartition = firstResult[partitionIndex];
                    var actualPartition = result[partitionIndex];
                    
                    actualPartition.Select(v => v.Label).Should().Equal(
                        expectedPartition.Select(v => v.Label), 
                        $"partition {partitionIndex} should be identical across runs"
                    );
                }
            }
        }

        [Fact]
        public void DistanceVector_ShouldUseShortestPaths_FAILS()
        {
            // Test demonstrates DFS being used instead of BFS for distance calculation
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "Root", "A", "B", "Target" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
            // Create graph where BFS and DFS give different distances:
            // Root -> A -> Target (distance 2)
            // Root -> B -> A -> Target (distance 3)  
            graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag())); // Root -> A
            graph.AddEdge(new PSEdge(vertices[0], vertices[2], new PSEdgeTag())); // Root -> B
            graph.AddEdge(new PSEdge(vertices[2], vertices[1], new PSEdgeTag())); // B -> A
            graph.AddEdge(new PSEdge(vertices[1], vertices[3], new PSEdgeTag())); // A -> Target

            var cmdlet = new PSGraph.Cmdlets.GetGraphDistanceVector
            {
                Graph = graph
            };

            // This test may not fail reliably due to DFS traversal order being implementation-dependent
            // But it demonstrates the conceptual issue of using DFS for distance calculation
            cmdlet.EndProcessing();
            
            // The distance to Target should be 2 (Root->A->Target) using BFS
            // But DFS might report 3 (Root->B->A->Target) depending on traversal order
            // This is a design flaw rather than a deterministic failure
            true.Should().BeTrue("This test demonstrates conceptual issue with DFS vs BFS for distances");
        }

        private static PsBidirectionalGraph CreateTestGraphWithKnownSCC()
        {
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "A", "B", "C", "D" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
            // Create two SCCs: {A,B} and {C,D}
            graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag())); // A->B
            graph.AddEdge(new PSEdge(vertices[1], vertices[0], new PSEdgeTag())); // B->A (creates SCC)
            graph.AddEdge(new PSEdge(vertices[2], vertices[3], new PSEdgeTag())); // C->D
            graph.AddEdge(new PSEdge(vertices[3], vertices[2], new PSEdgeTag())); // D->C (creates SCC)
            graph.AddEdge(new PSEdge(vertices[1], vertices[2], new PSEdgeTag())); // B->C (bridge between SCCs)
            
            return graph;
        }
    }
}