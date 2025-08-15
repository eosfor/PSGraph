using MathNet.Numerics.LinearAlgebra;
using Xunit;
using PSGraph.DesignStructureMatrix;
using FluentAssertions;
using PSGraph.Model;
using System.Diagnostics.CodeAnalysis; 

namespace PSGraph.Tests
{
    public class DsmPartitioningTests
    {

        [ExcludeFromCodeCoverage]
        private Matrix<double> ReorderMatrix(Matrix<double> original, Dictionary<PSVertex, int> actualIndex, string[] expectedOrder)
        {
            int size = expectedOrder.Length;
            var reordered = Matrix<double>.Build.Dense(size, size);

            for (int i = 0; i < size; i++)
            {
                var rowVertex = new PSVertex(expectedOrder[i]);
                int sourceRow = actualIndex[rowVertex];

                for (int j = 0; j < size; j++)
                {
                    var colVertex = new PSVertex(expectedOrder[j]);
                    int sourceCol = actualIndex[colVertex];

                    reordered[i, j] = original[sourceRow, sourceCol];
                }
            }

            return reordered;
        }

        [ExcludeFromCodeCoverage]
        private void AssertVertexGroupsAreClustered(Dictionary<PSVertex, int> rowIndex, List<List<PSVertex>> expectedGroups)
        {
            var allIndices = new Dictionary<string, int>();
            foreach (var kvp in rowIndex)
                allIndices[kvp.Key.Name] = kvp.Value;

            for (int groupIdx = 0; groupIdx < expectedGroups.Count; groupIdx++)
            {
                var group = expectedGroups[groupIdx];
                var indices = group.Select(v => allIndices[v.Label]).OrderBy(i => i).ToList();

                // Все индексы должны быть рядом
                for (int i = 1; i < indices.Count; i++)
                {
                    (indices[i] - indices[i - 1]).Should().BeLessThanOrEqualTo(1,
                        $"vertices in group {groupIdx} ({string.Join(",", group)}) should be adjacent");
                }

                // Между группами индексы должны быть дальше
                for (int j = groupIdx + 1; j < expectedGroups.Count; j++)
                {
                    var otherGroup = expectedGroups[j];
                    var otherIndices = otherGroup.Select(v => allIndices[v.Label]).OrderBy(i => i).ToList();

                    indices.Last().Should().BeLessThan(otherIndices.First(),
                        $"group {groupIdx} should come before group {j}");
                }
            }
        }

        [Fact]
        public void BasicPowersLoopDetection_ShouldDetectLoopsCorrectly()
        {
            var dsm = new DsmClassic(GraphTestData.DSMFull);
            dsm.Should().NotBeNull();

            var algo = new DsmPowersLoopDetectionAlgorithm(dsm);

            // Проверяем DetectLoops
            var blocks = algo.DetectLoops(dsm);
            blocks.Should().NotBeNull();
            blocks.Count.Should().BeGreaterThan(0);

            // Проверяем CondenceLoops
            List<List<PSVertex>> collapsedBlocks;
            var condensedDsm = algo.CondenceLoops(dsm, out collapsedBlocks);
            condensedDsm.Should().NotBeNull();
            collapsedBlocks.Should().BeEquivalentTo(blocks);

            // condensedDsm не содержит вершин без входов/выходов
            var sinks = condensedDsm.GetSinks();
            var sources = condensedDsm.GetSources();
            sinks.Count.Should().Be(0);
            sources.Count.Should().Be(0);
        }
    }
}
