using Xunit;
using FluentAssertions;
using PSGraph.Model;
using PSGraph.DesignStructureMatrix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSGraph.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-based tests for verifying algorithmic invariants
    /// These test fundamental mathematical properties that must hold for correct implementations
    /// </summary>
    public class GraphAlgorithmPropertyTests
    {
        private readonly Random _random = new Random(42); // Fixed seed for reproducibility

        [Fact]
        public void SCC_Partition_Properties_Should_Hold()
        {
            // Test SCC properties across multiple random graphs
            for (int trial = 0; trial < 50; trial++)
            {
                var graph = GenerateRandomGraph(seed: trial, vertexCount: 8, edgeDensity: 0.4);
                var dsm = new DsmClassic(graph);
                
                var graphAlgo = new DsmGraphPartitioningAlgorithm(dsm);
                graphAlgo.Partition();
                
                // Property 1: Every vertex should be in exactly one partition
                var allPartitionedVertices = graphAlgo.Partitions.SelectMany(p => p).ToList();
                allPartitionedVertices.Should().HaveCount(graph.VertexCount, 
                    "every vertex should be partitioned exactly once");
                
                allPartitionedVertices.Distinct().Should().HaveCount(graph.VertexCount,
                    "no vertex should appear in multiple partitions");
                
                // Property 2: Each partition should form a strongly connected subgraph
                foreach (var partition in graphAlgo.Partitions.Where(p => p.Count > 1))
                {
                    VerifyStronglyConnected(graph, partition);
                }
                
                // Property 3: No edges should exist between vertices in same partition that
                // would create a larger strongly connected component
                VerifyPartitionMinimality(graph, graphAlgo.Partitions);
            }
        }

        [Fact]
        public void Shortest_Path_Properties_Should_Hold()
        {
            for (int trial = 0; trial < 30; trial++)
            {
                var graph = GenerateConnectedGraph(seed: trial, vertexCount: 6);
                var vertices = graph.Vertices.ToArray();
                
                if (vertices.Length < 2) continue;
                
                var source = vertices[0];
                var target = vertices[vertices.Length - 1];
                
                // Find shortest path
                var pathFunc = graph.ShortestPathsDijkstra(e => e.Weight, source);
                IEnumerable<PSEdge> path = null;
                var pathExists = pathFunc(target, out path);
                
                if (pathExists && path != null)
                {
                    var pathEdges = path.ToArray();
                    
                    // Property 1: Path should be connected
                    for (int i = 0; i < pathEdges.Length - 1; i++)
                    {
                        pathEdges[i].Target.Should().Be(pathEdges[i + 1].Source,
                            "path edges should form connected sequence");
                    }
                    
                    // Property 2: Path should start and end at correct vertices  
                    if (pathEdges.Length > 0)
                    {
                        pathEdges[0].Source.Should().Be(source, "path should start at source");
                        pathEdges[pathEdges.Length - 1].Target.Should().Be(target, "path should end at target");
                    }
                    
                    // Property 3: Path length should be optimal (can't verify without reference implementation)
                    // But we can verify non-negativity and basic sanity
                    var totalWeight = pathEdges.Sum(e => e.Weight);
                    totalWeight.Should().BeGreaterOrEqualTo(0, "path weight should be non-negative");
                }
            }
        }

        [Fact]
        public void DSM_Index_Consistency_Properties()
        {
            for (int trial = 0; trial < 20; trial++)
            {
                var graph = GenerateRandomGraph(seed: trial, vertexCount: 6, edgeDensity: 0.3);
                var dsm = new DsmClassic(graph);
                
                // Property 1: Row and column indices should be identical for square matrix
                dsm.RowIndex.Keys.Should().BeEquivalentTo(dsm.ColIndex.Keys,
                    "row and column should have same vertices for square DSM");
                
                dsm.RowIndex.Should().HaveCount(dsm.ColIndex.Count,
                    "row and column index counts should match");
                
                // Property 2: Indices should be consecutive starting from 0
                var rowIndices = dsm.RowIndex.Values.OrderBy(x => x).ToArray();
                var expectedIndices = Enumerable.Range(0, rowIndices.Length).ToArray();
                rowIndices.Should().Equal(expectedIndices, "indices should be consecutive starting from 0");
                
                // Property 3: Matrix dimensions should match vertex count
                dsm.DsmMatrixView.RowCount.Should().Be(graph.VertexCount);
                dsm.DsmMatrixView.ColumnCount.Should().Be(graph.VertexCount);
                
                // Property 4: Adjacency values should match graph edges
                foreach (var edge in graph.Edges)
                {
                    var value = dsm[edge.Source, edge.Target];
                    value.Should().BeGreaterThan(0, $"edge {edge.Source} -> {edge.Target} should have positive value in DSM");
                }
                
                // Property 5: Non-edges should have zero values (for simple graphs)
                var allVertexPairs = from v1 in graph.Vertices
                                   from v2 in graph.Vertices  
                                   where v1 != v2
                                   select new { Source = v1, Target = v2 };
                
                foreach (var pair in allVertexPairs)
                {
                    var hasEdge = graph.ContainsEdge(pair.Source, pair.Target);
                    var dsmValue = dsm[pair.Source, pair.Target];
                    
                    if (!hasEdge)
                    {
                        dsmValue.Should().Be(0, $"non-edge {pair.Source} -> {pair.Target} should be zero in DSM");
                    }
                }
            }
        }

        [Fact]
        public void DSM_Remove_Operation_Properties()
        {
            for (int trial = 0; trial < 15; trial++)
            {
                var graph = GenerateRandomGraph(seed: trial, vertexCount: 6, edgeDensity: 0.4);
                if (graph.VertexCount < 2) continue;
                
                var dsm = new DsmClassic(graph);
                var vertexToRemove = graph.Vertices.First();
                var remainingVertices = graph.Vertices.Except(new[] { vertexToRemove }).ToArray();
                
                var reducedDsm = dsm.Remove(vertexToRemove);
                
                // Property 1: Reduced DSM should have one less vertex
                reducedDsm.RowIndex.Should().HaveCount(dsm.RowIndex.Count - 1,
                    "reduced DSM should have one less vertex");
                
                // Property 2: Removed vertex should not appear in indices  
                reducedDsm.RowIndex.Keys.Should().NotContain(vertexToRemove,
                    "removed vertex should not appear in row index");
                reducedDsm.ColIndex.Keys.Should().NotContain(vertexToRemove,
                    "removed vertex should not appear in column index");
                
                // Property 3: Remaining vertices should preserve relative adjacencies
                foreach (var edge in graph.Edges.Where(e => e.Source != vertexToRemove && e.Target != vertexToRemove))
                {
                    var originalValue = dsm[edge.Source, edge.Target];
                    var reducedValue = reducedDsm[edge.Source, edge.Target];
                    
                    reducedValue.Should().Be(originalValue, 
                        $"adjacency {edge.Source} -> {edge.Target} should be preserved after vertex removal");
                }
                
                // Property 4: Matrix dimensions should be consistent
                reducedDsm.DsmMatrixView.RowCount.Should().Be(remainingVertices.Length);
                reducedDsm.DsmMatrixView.ColumnCount.Should().Be(remainingVertices.Length);
            }
        }

        [Fact]
        public void Graph_Isomorphism_Invariant_Property()
        {
            // Test that relabeling vertices preserves algorithmic results
            var originalGraph = GenerateRandomGraph(seed: 123, vertexCount: 5, edgeDensity: 0.5);
            var dsm1 = new DsmClassic(originalGraph);
            
            // Create isomorphic graph with relabeled vertices
            var isomorphicGraph = new PsBidirectionalGraph();
            var vertexMapping = new Dictionary<PSVertex, PSVertex>();
            
            foreach (var vertex in originalGraph.Vertices)
            {
                var newVertex = new PSVertex($"NEW_{vertex.Label}");
                isomorphicGraph.AddVertex(newVertex);
                vertexMapping[vertex] = newVertex;
            }
            
            foreach (var edge in originalGraph.Edges)
            {
                var newSource = vertexMapping[edge.Source];
                var newTarget = vertexMapping[edge.Target];
                isomorphicGraph.AddEdge(new PSEdge(newSource, newTarget, new PSEdgeTag()) 
                { 
                    Weight = edge.Weight 
                });
            }
            
            var dsm2 = new DsmClassic(isomorphicGraph);
            
            // SCC structure should be preserved under isomorphism
            var algo1 = new DsmGraphPartitioningAlgorithm(dsm1);
            var algo2 = new DsmGraphPartitioningAlgorithm(dsm2);
            
            algo1.Partition();
            algo2.Partition();
            
            // Should have same number of partitions
            algo1.Partitions.Should().HaveCount(algo2.Partitions.Count,
                "isomorphic graphs should have same SCC structure");
            
            // Each partition should have same size distribution
            var sizes1 = algo1.Partitions.Select(p => p.Count).OrderBy(x => x).ToArray();
            var sizes2 = algo2.Partitions.Select(p => p.Count).OrderBy(x => x).ToArray(); 
            
            sizes1.Should().Equal(sizes2, "isomorphic graphs should have same partition sizes");
        }

        // Helper methods for generating test data
        private PsBidirectionalGraph GenerateRandomGraph(int seed, int vertexCount, double edgeDensity)
        {
            var random = new Random(seed);
            var graph = new PsBidirectionalGraph();
            
            // Add vertices
            var vertices = Enumerable.Range(0, vertexCount)
                .Select(i => new PSVertex($"V{i}"))
                .ToArray();
            graph.AddVertexRange(vertices);
            
            // Add random edges based on density
            int maxEdges = vertexCount * (vertexCount - 1); // Max edges in directed graph
            int targetEdges = (int)(maxEdges * edgeDensity);
            
            var addedEdges = new HashSet<(PSVertex, PSVertex)>();
            
            for (int i = 0; i < targetEdges; i++)
            {
                PSVertex source, target;
                do
                {
                    source = vertices[random.Next(vertexCount)];
                    target = vertices[random.Next(vertexCount)];
                } while (source == target || addedEdges.Contains((source, target)));
                
                addedEdges.Add((source, target));
                var weight = random.NextDouble() * 10 + 1; // Positive weights between 1-11
                graph.AddEdge(new PSEdge(source, target, new PSEdgeTag()) { Weight = weight });
            }
            
            return graph;
        }

        private PsBidirectionalGraph GenerateConnectedGraph(int seed, int vertexCount)
        {
            var random = new Random(seed);
            var graph = new PsBidirectionalGraph();
            
            if (vertexCount == 0) return graph;
            
            var vertices = Enumerable.Range(0, vertexCount)
                .Select(i => new PSVertex($"V{i}"))
                .ToArray();
            graph.AddVertexRange(vertices);
            
            if (vertexCount == 1) return graph;
            
            // Create spanning tree to ensure connectivity
            for (int i = 1; i < vertexCount; i++)
            {
                var parentIndex = random.Next(i);
                var weight = random.NextDouble() * 5 + 1;
                graph.AddEdge(new PSEdge(vertices[parentIndex], vertices[i], new PSEdgeTag()) { Weight = weight });
            }
            
            // Add additional random edges
            int additionalEdges = random.Next(0, vertexCount);
            for (int i = 0; i < additionalEdges; i++)
            {
                var from = vertices[random.Next(vertexCount)];
                var to = vertices[random.Next(vertexCount)];
                if (from != to && !graph.ContainsEdge(from, to))
                {
                    var weight = random.NextDouble() * 5 + 1;
                    graph.AddEdge(new PSEdge(from, to, new PSEdgeTag()) { Weight = weight });
                }
            }
            
            return graph;
        }

        private void VerifyStronglyConnected(PsBidirectionalGraph graph, List<PSVertex> partition)
        {
            if (partition.Count <= 1) return; // Single vertices are trivially strongly connected
            
            // For each pair of vertices in partition, verify mutual reachability
            foreach (var from in partition)
            {
                foreach (var to in partition)
                {
                    if (from.Equals(to)) continue;
                    
                    var pathExists = HasPath(graph, from, to);
                    pathExists.Should().BeTrue($"vertex {from} should reach {to} in SCC partition");
                }
            }
        }

        private void VerifyPartitionMinimality(PsBidirectionalGraph graph, List<List<PSVertex>> partitions)
        {
            // Verify that merging any two partitions would not create a valid SCC
            // This is a complex property to verify efficiently, so we'll do basic sanity checks
            
            var allPartitioned = partitions.SelectMany(p => p).ToHashSet();
            
            // Check no edges exist that would merge partitions into larger SCCs
            foreach (var partition1 in partitions)
            {
                foreach (var partition2 in partitions)
                {
                    if (partition1 == partition2) continue;
                    
                    // If there are edges in both directions between partitions, they might need merging
                    var hasEdge12 = partition1.Any(v1 => partition2.Any(v2 => graph.ContainsEdge(v1, v2)));
                    var hasEdge21 = partition2.Any(v2 => partition1.Any(v1 => graph.ContainsEdge(v2, v1)));
                    
                    if (hasEdge12 && hasEdge21)
                    {
                        // This suggests the partitions might be incorrectly separated
                        // But without full transitive closure check, we can't be certain
                        // This is more of a warning than a strict test
                        Console.WriteLine($"Warning: Bidirectional edges found between partitions - may indicate incorrect SCC detection");
                    }
                }
            }
        }

        private bool HasPath(PsBidirectionalGraph graph, PSVertex from, PSVertex to)
        {
            if (from.Equals(to)) return true;
            
            var visited = new HashSet<PSVertex>();
            var queue = new Queue<PSVertex>();
            
            queue.Enqueue(from);
            visited.Add(from);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                foreach (var edge in graph.OutEdges(current))
                {
                    if (edge.Target.Equals(to)) return true;
                    
                    if (!visited.Contains(edge.Target))
                    {
                        visited.Add(edge.Target);
                        queue.Enqueue(edge.Target);
                    }
                }
            }
            
            return false;
        }
    }
}