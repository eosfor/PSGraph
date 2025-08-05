using MathNet.Numerics.LinearAlgebra;
using Xunit;
using PSGraph.DesignStructureMatrix;
using FluentAssertions;
using PSGraph.Model;

namespace PSGraph.Tests
{
    public class DsmPartitioningTests
    {

        private Matrix<float> ReorderMatrix(Matrix<float> original, Dictionary<PSVertex, int> actualIndex, string[] expectedOrder)
        {
            int size = expectedOrder.Length;
            var reordered = Matrix<float>.Build.Dense(size, size);

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
        public void BasicPartitioning_ShouldPartitionCorrectly()
        {
            var dsm = new DsmClassic(GraphTestData.DSMFull);
            dsm.Should().NotBeNull("DSM object should not be null");

            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var result = algo.Partition();

            result.Should().NotBeNull("Partitioning result should not be null");


            // Expected DSM after partitioning
            float[,] expectedMatrix = {
                                { 0, 0, 0, 0, 0, 0, 0 },
                                { 0, 0, 0, 1, 0, 0, 0 },
                                { 0, 1, 0, 0, 0, 0, 0 },
                                { 1, 0, 1, 0, 0, 0, 0 },
                                { 0, 0, 1, 0, 0, 1, 0 },
                                { 1, 1, 0, 1, 1, 0, 0 },
                                { 1, 0, 0, 0, 0, 1, 0 }
                              };

            // Expected row/column order after partitioning
            string[] expectedOrder = { "F", "B", "D", "G", "A", "C", "E" };

            // Задаем ожидаемые группы
            var expectedGroups = new List<List<PSVertex>>
            {
                new() { new PSVertex("F"), new PSVertex("E") },
                new() { new PSVertex("B"), new PSVertex("D"), new PSVertex("G") },
                new() { new PSVertex("A"), new PSVertex("C") }
            };

            Console.WriteLine("Row indices:");
            foreach (var kvp in result.RowIndex.OrderBy(k => k.Value))
            {
                Console.WriteLine($"{kvp.Key.Name} => {kvp.Value}");
            }

            // Проверим, что группы упорядочены и внутри сжаты
            AssertVertexGroupsAreClustered(result.RowIndex, expectedGroups);

            var reorderedActual = ReorderMatrix(result.DsmMatrixView, result.RowIndex, expectedOrder);

            // Create expected matrix using MathNet
            var targetMatrix = Matrix<Single>.Build.DenseOfArray(expectedMatrix);

            // Check if the partitioned DSM matrix matches the expected matrix
            reorderedActual.Should().BeEquivalentTo(targetMatrix, "The partitioned DSM matrix should match the expected matrix");


            // Validate row and column indices match the expected vertex order
            for (int idx = 0; idx < expectedOrder.Length; idx++)
            {
                var vertex = new PSVertex(expectedOrder[idx]);

                result.RowIndex.Should().ContainKey(vertex);
                result.ColIndex.Should().ContainKey(vertex);
            }

            for (int i = 0; i < reorderedActual.RowCount; i++)
            {
                reorderedActual[i, i].Should().Be(0, $"self-dependency at index {i} should be zero");
            }
        }
    }
}
