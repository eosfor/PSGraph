# PSGraph Algorithmic Correctness Review

## Executive Summary

### Critical Risks to Correctness (High Priority Fixes):
• **DSM Matrix Power Calculation** (DsmClassicPartitioningAlgorithm.cs:90): Raises matrix to consecutive powers without optimization, causing O(n^4) complexity with potential stack overflow
• **SCC Detection Missing Determinism** (DsmGraphPartitioningAlgorithm.cs:69): Groups components non-deterministically, breaking repeatability requirements
• **Index Mapping Inconsistencies** (DsmBase.cs:67-84): Row/column index updates during vertex removal have potential off-by-one errors and race conditions
• **Missing Input Validation** (GetGraphPath.cs:31): No validation that source/target vertices exist in graph before algorithm execution
• **Dijkstra Weight Function Unchecked** (GetGraphPath.cs:31): Edge weight function doesn't validate for negative weights or NaN/Infinity values  
• **Distance Vector Root Selection** (GetGraphDistanceVector.cs:26): Uses arbitrary root vertices without handling strongly connected components properly
• **Memory Leaks in Matrix Operations** (DsmBase.cs:100): Matrix operations create new dense matrices without disposing previous instances
• **No Cycle Detection in Topological Context**: DSM reordering doesn't validate DAG properties before applying topological semantics
• **Partition Size Bounds Missing**: No upper bounds checking on partition sizes, could cause memory exhaustion
• **Concurrent Access Unsafe**: Dictionary operations in indexing not thread-safe

## Algorithms Reviewed

### 1. DSM Classic Partitioning Algorithm
**File**: `/PSGraph/DSM/DsmClassicPartitioningAlgorithm.cs`  
**Purpose**: Detects strongly connected components (SCCs) in DSM using matrix power method  
**Reference Method**: Matrix power-based SCC detection (not standard Tarjan/Kosaraju)

#### Correctness Analysis:
- **Invariants**: Matrix powers should reveal cycles through non-zero diagonal elements
- **Pre-conditions**: Square adjacency matrix, valid vertex-to-index mapping
- **Post-conditions**: Partitioned vertices grouped by SCC membership
- **Critical Flaw**: Algorithm raises matrix to consecutive powers from 2 to n (line 88), but this is inefficient and incorrect for SCC detection. True SCCs require transitive closure, not just powers.

**Complexity Analysis**: O(n^4) time, O(n^3) space - far exceeding expected O(n+m) for proper SCC algorithms

**Determinism**: Non-deterministic due to hash set ordering (line 25) and dictionary iteration order

**Edge Cases**:
- ✗ Self-loops not handled correctly (diagonal elements)
- ✗ Disconnected components not properly isolated
- ✗ Empty graphs will cause division by zero in power operations

**Findings**: 
- Lines 88-110: Incorrect SCC detection logic
- Line 25: Non-deterministic set ordering
- Line 94: Diagonal sum could be zero for valid SCCs with cycle length > matrix size

### 2. DSM Graph-Based Partitioning Algorithm  
**File**: `/PSGraph/DSM/DsmGraphPartitioningAlgorithm.cs`  
**Purpose**: Detects SCCs using QuikGraph's StronglyConnectedComponentsAlgorithm  
**Reference Method**: Matches standard SCC algorithms (likely Tarjan-based)

#### Correctness Analysis:
- **Invariants**: Components dictionary maps vertex to component ID
- **Post-conditions**: All vertices assigned to exactly one component
- **Issue**: Line 69 groups by component value but doesn't ensure deterministic ordering within components

**Complexity Analysis**: O(V + E) as expected for SCC detection

**Determinism**: ✗ GroupBy operation doesn't guarantee consistent ordering between runs

**Findings**: 
- Line 29: Sorts by degree, but degree calculation uses different graph view than partitioning
- Line 69: Non-deterministic grouping could produce different vertex orderings

### 3. Dijkstra Shortest Path
**File**: `/PSGraph/cmdlets/graph/GetGraphPath.cs`  
**Purpose**: Finds shortest path between two vertices using Dijkstra's algorithm  
**Reference Method**: Standard Dijkstra implementation (delegated to QuikGraph)

#### Correctness Analysis:
- **Pre-conditions**: Non-negative edge weights, vertices exist in graph
- **Post-conditions**: Returns shortest path edges or null if no path exists
- **Critical Flaw**: No validation of edge weights or vertex membership

**Complexity Analysis**: O((V + E) log V) as expected

**Determinism**: ✓ Should be deterministic given same input graph

**Findings**:
- Line 31: No validation that edge weights are non-negative
- Line 32: No check if vertices exist in graph before algorithm execution
- Missing: No handling of NaN or Infinity weights

### 4. Distance Vector Calculation  
**File**: `/PSGraph/cmdlets/graph/GetGraphDistanceVector.cs`  
**Purpose**: Calculates distance from root vertices using DFS  
**Reference Method**: DFS-based level calculation

#### Correctness Analysis:
- **Issue**: Uses DFS instead of BFS for distance calculation, which doesn't guarantee shortest distances
- **Root Selection**: Line 26 selects vertices with in-degree 0, but doesn't handle SCCs properly

**Complexity Analysis**: O(V + E) per root vertex

**Findings**:
- Line 26: Root selection logic fails for strongly connected graphs
- Line 24: Uses vertex count constant instead of edge weights for distances
- Missing: No handling of unreachable vertices

### 5. DSM Index Management
**File**: `/PSGraph/DSM/DsmBase.cs`  
**Purpose**: Maintains mapping between vertices and matrix indices  
**Reference Method**: Custom index management

#### Correctness Analysis:
- **Index Updates**: Lines 73-82 update indices after vertex removal
- **Critical Issue**: Index decrementing logic could create gaps or duplicates

**Findings**:
- Lines 73-82: Off-by-one potential in index updates
- Line 100: Matrix reordering doesn't validate index consistency
- Missing: No bounds checking on matrix access operations

## Failing Test Skeletons

```csharp
using Xunit;
using FluentAssertions;
using PSGraph.Model;
using PSGraph.DesignStructureMatrix;
using System.Collections.Generic;
using System.Linq;

namespace PSGraph.Tests.AlgorithmicCorrectnessTests
{
    public class DSMPartitioningCorrectnessTests
    {
        [Fact]
        public void ClassicPartitioning_ShouldBeRepeatable()
        {
            // Test for deterministic SCC detection
            var graph = CreateTestGraphWithKnownSCC();
            var dsm = new DsmClassic(graph);
            var algo1 = new DsmClassicPartitioningAlgorithm(dsm);
            var algo2 = new DsmClassicPartitioningAlgorithm(dsm);

            var result1 = algo1.Partition();
            var result2 = algo2.Partition();

            // Should fail due to non-deterministic hash set ordering
            result1.RowIndex.Should().Equal(result2.RowIndex);
        }

        [Fact] 
        public void ClassicPartitioning_ShouldDetectSimpleCycle()
        {
            // Test basic SCC detection correctness
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "A", "B", "C" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
            // Create cycle: A -> B -> C -> A
            graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag()));
            graph.AddEdge(new PSEdge(vertices[1], vertices[2], new PSEdgeTag()));
            graph.AddEdge(new PSEdge(vertices[2], vertices[0], new PSEdgeTag()));

            var dsm = new DsmClassic(graph);
            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var result = algo.Partition();

            // Should fail - classic algorithm may not detect this 3-cycle correctly
            algo.Partitions.Should().ContainSingle(partition => partition.Count == 3);
        }

        [Fact]
        public void GraphBasedPartitioning_ShouldBeDeterministic() 
        {
            var graph = CreateTestGraphWithKnownSCC();
            var dsm = new DsmClassic(graph);

            var results = new List<IDsm>();
            for (int i = 0; i < 10; i++)
            {
                var algo = new DsmGraphPartitioningAlgorithm(dsm);
                results.Add(algo.Partition());
            }

            // Should fail due to GroupBy non-deterministic ordering
            var firstResult = results[0];
            foreach (var result in results.Skip(1))
            {
                result.RowIndex.Should().Equal(firstResult.RowIndex);
            }
        }
    }

    public class ShortestPathCorrectnessTests
    {
        [Fact]
        public void GetGraphPath_ShouldValidateVertexMembership()
        {
            var graph = new PsBidirectionalGraph();
            var existingVertex = new PSVertex("A");
            var nonExistentVertex = new PSVertex("B");
            graph.AddVertex(existingVertex);

            var cmdlet = new PSGraph.Cmdlets.GetGraphPath
            {
                From = existingVertex,
                To = nonExistentVertex,
                Graph = graph
            };

            // Should fail - no validation of vertex membership
            Action act = () => cmdlet.ProcessRecord();
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetGraphPath_ShouldHandleNegativeWeights()
        {
            var graph = new PsBidirectionalGraph();
            var vertices = new[] { "A", "B" }.Select(s => new PSVertex(s)).ToArray();
            graph.AddVertexRange(vertices);
            
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

            // Should fail - Dijkstra doesn't work with negative weights
            Action act = () => cmdlet.ProcessRecord();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class DSMIndexConsistencyTests
    {
        [Fact]
        public void DSMRemove_ShouldMaintainIndexConsistency()
        {
            var graph = new PsBidirectionalGraph();
            var vertices = Enumerable.Range(0, 5).Select(i => new PSVertex($"V{i}")).ToArray();
            graph.AddVertexRange(vertices);

            var dsm = new DsmClassic(graph);
            var originalIndices = dsm.RowIndex.Values.OrderBy(x => x).ToArray();

            var removedDsm = dsm.Remove(vertices[2]); // Remove middle vertex

            // Should fail - indices might have gaps or overlaps
            var newIndices = removedDsm.RowIndex.Values.OrderBy(x => x).ToArray();
            newIndices.Should().Equal(Enumerable.Range(0, 4));
        }

        [Fact]
        public void DSMOrder_ShouldPreserveAdjacencyValues()
        {
            var graph = CreateTestGraphWithEdges();
            var dsm = new DsmClassic(graph);
            
            var vertices = dsm.RowIndex.Keys.ToList();
            var shuffledOrder = vertices.OrderBy(_ => Guid.NewGuid()).ToList();
            
            var reorderedDsm = dsm.Order(shuffledOrder);

            // Verify adjacency relationships are preserved
            foreach (var edge in graph.Edges)
            {
                var originalValue = dsm[edge.Source, edge.Target];
                var reorderedValue = reorderedDsm[edge.Source, edge.Target];
                
                // Should pass but might fail due to index mapping issues
                reorderedValue.Should().Be(originalValue);
            }
        }
    }

    // Property-based testing skeleton
    public class PropertyBasedAlgorithmTests
    {
        [Fact]
        public void SCC_Soundness_Property()
        {
            // Generate random graphs and verify SCC properties
            for (int trial = 0; trial < 100; trial++)
            {
                var graph = GenerateRandomGraph(seed: trial);
                var dsm = new DsmClassic(graph);
                
                var classicAlgo = new DsmClassicPartitioningAlgorithm(dsm);
                var graphAlgo = new DsmGraphPartitioningAlgorithm(dsm);
                
                var classicResult = classicAlgo.Partition();
                var graphResult = graphAlgo.Partition();
                
                // Both algorithms should find same number of components
                classicAlgo.Partitions.Count.Should().Be(graphAlgo.Partitions.Count);
                
                // Each partition should be strongly connected
                foreach (var partition in graphAlgo.Partitions)
                {
                    VerifyStrongConnectivity(graph, partition);
                }
            }
        }

        private PsBidirectionalGraph GenerateRandomGraph(int seed)
        {
            var random = new Random(seed);
            var graph = new PsBidirectionalGraph();
            var vertexCount = random.Next(5, 20);
            
            var vertices = Enumerable.Range(0, vertexCount)
                .Select(i => new PSVertex($"V{i}"))
                .ToArray();
            graph.AddVertexRange(vertices);
            
            // Add random edges to create cycles and components
            var edgeCount = random.Next(vertexCount, vertexCount * 2);
            for (int i = 0; i < edgeCount; i++)
            {
                var from = vertices[random.Next(vertexCount)];
                var to = vertices[random.Next(vertexCount)];
                if (!graph.ContainsEdge(from, to))
                {
                    graph.AddEdge(new PSEdge(from, to, new PSEdgeTag()));
                }
            }
            
            return graph;
        }

        private void VerifyStrongConnectivity(PsBidirectionalGraph graph, List<PSVertex> partition)
        {
            // Verify each vertex in partition can reach every other vertex
            foreach (var from in partition)
            {
                foreach (var to in partition)
                {
                    if (from != to)
                    {
                        var pathExists = HasPath(graph, from, to);
                        pathExists.Should().BeTrue($"No path from {from} to {to} in SCC");
                    }
                }
            }
        }

        private bool HasPath(PsBidirectionalGraph graph, PSVertex from, PSVertex to)
        {
            // Simple BFS path existence check
            var visited = new HashSet<PSVertex>();
            var queue = new Queue<PSVertex>();
            queue.Enqueue(from);
            visited.Add(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Equals(to)) return true;

                foreach (var edge in graph.OutEdges(current))
                {
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

    // Helper methods
    private static PsBidirectionalGraph CreateTestGraphWithKnownSCC()
    {
        var graph = new PsBidirectionalGraph();
        var vertices = new[] { "A", "B", "C", "D" }.Select(s => new PSVertex(s)).ToArray();
        graph.AddVertexRange(vertices);
        
        // Create two SCCs: {A,B} and {C,D}
        graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag())); // A->B
        graph.AddEdge(new PSEdge(vertices[1], vertices[0], new PSEdgeTag())); // B->A
        graph.AddEdge(new PSEdge(vertices[2], vertices[3], new PSEdgeTag())); // C->D  
        graph.AddEdge(new PSEdge(vertices[3], vertices[2], new PSEdgeTag())); // D->C
        graph.AddEdge(new PSEdge(vertices[1], vertices[2], new PSEdgeTag())); // B->C (bridge)
        
        return graph;
    }

    private static PsBidirectionalGraph CreateTestGraphWithEdges()
    {
        var graph = new PsBidirectionalGraph();
        var vertices = new[] { "A", "B", "C" }.Select(s => new PSVertex(s)).ToArray();
        graph.AddVertexRange(vertices);
        
        graph.AddEdge(new PSEdge(vertices[0], vertices[1], new PSEdgeTag()) { Weight = 1.0 });
        graph.AddEdge(new PSEdge(vertices[1], vertices[2], new PSEdgeTag()) { Weight = 2.0 });
        graph.AddEdge(new PSEdge(vertices[2], vertices[0], new PSEdgeTag()) { Weight = 3.0 });
        
        return graph;
    }
}
```

## Verification & Fuzzing Plan

### Oracle/Reference Validation:
1. **SCC Detection**: Compare against NetworkX or LEMON library implementations using same input graphs
2. **Shortest Paths**: Validate against Boost.Graph Dijkstra with identical edge weights
3. **Matrix Operations**: Cross-check DSM operations against NumPy/SciPy matrix computations

### Metamorphic Relations:
1. **Graph Isomorphism**: Relabeling vertices should preserve SCC structure and shortest path lengths
2. **Edge Addition**: Adding non-bridge edges shouldn't change existing SCC membership  
3. **Weight Scaling**: Multiplying all edge weights by positive constant shouldn't change shortest paths (only total distances)
4. **Transpose Equivalence**: SCC structure should be identical in graph and its transpose

### Fuzzing Strategy:
```csharp
public class AlgorithmFuzzTesting
{
    [Fact] 
    public void FuzzSCCDetection()
    {
        for (int seed = 0; seed < 1000; seed++)
        {
            var graph = GenerateRandomDAG(seed);
            var dsm = new DsmClassic(graph);
            
            // Add random back-edges to create SCCs
            AddRandomBackEdges(graph, seed);
            
            var classicAlgo = new DsmClassicPartitioningAlgorithm(dsm);
            var graphAlgo = new DsmGraphPartitioningAlgorithm(dsm);
            
            // Both should produce valid partitions
            Action classicAction = () => classicAlgo.Partition();
            Action graphAction = () => graphAlgo.Partition();
            
            classicAction.Should().NotThrow();
            graphAction.Should().NotThrow();
            
            // Cross-validate results
            ValidateSCCProperties(graph, classicAlgo.Partitions);
            ValidateSCCProperties(graph, graphAlgo.Partitions);
        }
    }
    
    [Fact]
    public void FuzzShortestPaths() 
    {
        for (int seed = 0; seed < 500; seed++)
        {
            var graph = GenerateRandomGraph(seed);
            var vertices = graph.Vertices.ToArray();
            
            if (vertices.Length < 2) continue;
            
            var from = vertices[seed % vertices.Length];
            var to = vertices[(seed + 1) % vertices.Length];
            
            var cmdlet = new GetGraphPath
            {
                From = from,
                To = to,
                Graph = graph
            };
            
            Action pathAction = () => cmdlet.ProcessRecord();
            pathAction.Should().NotThrow();
        }
    }
}
```

### Mutation Testing:
1. **Off-by-One Detection**: Modify index calculations by ±1 to catch boundary errors
2. **Orientation Errors**: Flip edge directions randomly to test algorithm robustness
3. **Weight Corruption**: Introduce NaN, Infinity, and negative weights to test validation

## Missing Features / Gaps

### High Priority:
1. **Weighted Edge Support**: DSM algorithms don't consider edge weights, only binary adjacency
2. **Self-Loop Policy**: Inconsistent handling of self-loops in different algorithms  
3. **Deterministic Ordering**: No seeded RNG for consistent tie-breaking in sorting operations
4. **Input Validation**: Missing null checks, bounds validation, and type verification
5. **Cancellation Support**: Long-running algorithms don't support cancellation tokens
6. **Memory Management**: No disposal of intermediate matrix operations causing memory leaks

### Medium Priority:
7. **Centrality Measures**: No betweenness, closeness, or eigenvector centrality implementations
8. **Topological Sort**: No explicit topological ordering algorithm despite DSM reordering use case  
9. **Cycle Detection**: No dedicated cycle detection beyond SCC algorithms
10. **Graph Metrics**: Missing density, clustering coefficient, diameter calculations

### Low Priority:
11. **Parallel Algorithms**: No multi-threaded implementations for large graphs
12. **Sparse Matrix Support**: Dense matrix assumption inefficient for sparse graphs
13. **Streaming Algorithms**: No support for dynamic/evolving graphs
14. **Visualization Export Validation**: No round-trip testing for Vega/JSON export formats

## Fix Suggestions

### 1. Fix SCC Determinism (High Priority)
```csharp
// In DsmGraphPartitioningAlgorithm.cs line 69:
// Replace:
var groups = algo.Components.GroupBy(v => v.Value).Select(v => v);

// With:
var groups = algo.Components
    .GroupBy(v => v.Value)
    .OrderBy(g => g.Key)  // Deterministic component ordering
    .Select(g => g.OrderBy(p => p.Key.Label).ToList()); // Deterministic vertex ordering
```

### 2. Add Input Validation for Dijkstra (High Priority)
```csharp
// In GetGraphPath.cs, add before line 31:
if (!Graph.ContainsVertex(From))
    throw new ArgumentException($"Graph does not contain source vertex {From}");
if (!Graph.ContainsVertex(To))
    throw new ArgumentException($"Graph does not contain target vertex {To}");

// Validate edge weights
foreach (var edge in Graph.Edges)
{
    if (double.IsNaN(edge.Weight) || double.IsNegativeInfinity(edge.Weight))
        throw new ArgumentException($"Graph contains invalid edge weight: {edge.Weight}");
    if (edge.Weight < 0)
        throw new ArgumentException("Dijkstra algorithm requires non-negative edge weights");
}
```

### 3. Fix Matrix Power SCC Algorithm (High Priority)  
```csharp
// Replace entire PartitionInternal method in DsmClassicPartitioningAlgorithm.cs:
private IEnumerable<List<PSVertex>> PartitionInternal(IDsm dsmObj)
{
    // Use Floyd-Warshall for transitive closure instead of powers
    var n = dsmObj.DsmMatrixView.RowCount;
    var transitiveClosure = dsmObj.DsmMatrixViewCopy;
    
    // Floyd-Warshall algorithm
    for (int k = 0; k < n; k++)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                transitiveClosure[i, j] = Math.Max(transitiveClosure[i, j], 
                    Math.Min(transitiveClosure[i, k], transitiveClosure[k, j]));
            }
        }
    }
    
    // Find SCCs using transitive closure
    var visited = new bool[n];
    
    for (int i = 0; i < n; i++)
    {
        if (visited[i]) continue;
        
        var component = new List<PSVertex>();
        for (int j = 0; j < n; j++)
        {
            if (!visited[j] && transitiveClosure[i, j] > 0 && transitiveClosure[j, i] > 0)
            {
                component.Add(dsmObj.RowIndex.First(kvp => kvp.Value == j).Key);
                visited[j] = true;
            }
        }
        
        if (component.Any())
            yield return component.OrderBy(v => v.Label).ToList(); // Deterministic ordering
    }
}
```

### 4. Fix Index Management (Medium Priority)
```csharp  
// In DsmBase.cs, replace index update logic (lines 73-82):
private static void UpdateIndicesAfterRemoval(Dictionary<PSVertex, int> indices, int removedIndex)
{
    var keysToUpdate = indices.Keys.Where(k => indices[k] > removedIndex).ToList();
    foreach (var key in keysToUpdate)
    {
        indices[key] = indices[key] - 1;
    }
    
    // Validate no gaps exist
    var sortedIndices = indices.Values.OrderBy(x => x).ToArray();
    for (int i = 0; i < sortedIndices.Length; i++)
    {
        if (sortedIndices[i] != i)
            throw new InvalidOperationException($"Index gap detected at position {i}");
    }
}
```

### 5. Add Memory Management (Medium Priority)
```csharp
// In DsmBase.cs, implement IDisposable:
public class DsmBase : IDsm, IDisposable
{
    // Add disposal of matrix resources
    public void Dispose()
    {
        _dsm?.Clear();
        _rowIndex?.Clear(); 
        _colIndex?.Clear();
        // Graph disposal handled by QuikGraph
    }
    
    // Update Order method to dispose intermediate matrices
    public IDsm Order(List<PSVertex> order)
    {
        using var dsmNew = Matrix<Single>.Build.Dense(_dsm.RowCount, _dsm.ColumnCount);
        // ... existing logic ...
        
        // Return new instance, original will be disposed by caller
        return new DsmBase(dsmNew, _graph, newRowIndex, newColIndex);
    }
}
```

## Risk Ranking

### High Risk:
1. **DSM Classic Partitioning Incorrect SCC Detection** - Algorithm fundamentally flawed, produces wrong results
2. **Non-Deterministic Results** - Breaks repeatability requirements for automated systems  
3. **Missing Input Validation** - Can cause runtime crashes or incorrect results
4. **Memory Leaks** - Will cause performance degradation in long-running processes

### Medium Risk: 
5. **Index Management Off-By-One** - Could corrupt data structures but has existing test coverage
6. **Distance Vector Root Selection** - Produces suboptimal but not incorrect results
7. **Missing Edge Weight Validation** - Dijkstra fails gracefully but gives wrong results

### Low Risk:
8. **Missing Centrality Algorithms** - Feature gap, not correctness issue
9. **Sparse Matrix Inefficiency** - Performance issue, not correctness
10. **No Cancellation Support** - Usability issue, algorithms still produce correct results

---

**Total Findings**: 15 concrete algorithmic issues identified across 5 major algorithm implementations
**Critical Path**: Fix SCC detection, add input validation, implement deterministic ordering
**Test Coverage**: 10+ failing test cases provided with property-based testing framework
**Verification**: Oracle comparison strategy defined with metamorphic relations specified