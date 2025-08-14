using Xunit;
using FluentAssertions;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using System.IO;

namespace PSGraph.Tests
{
    public class Graph5ClassicPartitioningTests
    {
        [Fact]
        public void Graph5_PowersLoopDetection_ShouldDetectLoopsCorrectly()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph5);
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

            // Итеративно конденсируем DSM, пока не останется элементов
            var currentDsm = condensedDsm;
            int maxIterations = 100;
            int iteration = 0;
            while (currentDsm.RowIndex.Count > 0 && iteration < maxIterations)
            {
                var sinks = currentDsm.GetSinks();
                var sources = currentDsm.GetSources();
                // Удаляем вершины без входов/выходов
                var toRemove = sinks.Union(sources).Distinct().ToList();
                if (toRemove.Count == 0)
                {
                    // Если нет вершин для удаления, пробуем конденсировать циклы
                    List<List<PSVertex>> innerBlocks;
                    var nextDsm = algo.CondenceLoops(currentDsm, out innerBlocks);
                    // Если DSM не изменился, прерываем цикл
                    if (nextDsm.RowIndex.Count == currentDsm.RowIndex.Count)
                        break;
                    currentDsm = nextDsm;
                }
                else
                {
                    currentDsm = currentDsm.Remove(toRemove);
                }
                iteration++;
            }

            // Проверяем, что цикл завершился корректно
            iteration.Should().BeLessThan(maxIterations, "DSM condensation should not loop forever");
        }
    }
}
