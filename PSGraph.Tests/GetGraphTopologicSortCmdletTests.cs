using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace PSGraph.Tests
{
    public class GetGraphTopologicSortCmdletTests : IDisposable
    {
        private PowerShell _powershell;

        public GetGraphTopologicSortCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphTopologicSort).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphTopologicSort_ShouldReturnTopologicallySortedVertices()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph6; // Using real graph data

            // Add the parameters for Get-GraphTopologicSort
            _powershell.AddCommand("Get-GraphTopologicSort")
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var sortedVertices = results[0].BaseObject as IEnumerable<PSVertex>;
            sortedVertices.Should().NotBeNull();

            // Check that vertices are sorted topologically.
            // In SimpleTestGraph5, A should appear before D and E, D and E before F, etc.
            var sortedList = sortedVertices.ToList();
            var vertexA = sortedList.FindIndex(v => v.Label == "A");
            var vertexD = sortedList.FindIndex(v => v.Label == "D");
            var vertexE = sortedList.FindIndex(v => v.Label == "E");
            var vertexF = sortedList.FindIndex(v => v.Label == "F");


            vertexA.Should().BeLessThan(vertexD);
            vertexA.Should().BeLessThan(vertexE);
            vertexD.Should().BeLessThan(vertexF);
            vertexE.Should().BeLessThan(vertexF);
        }

        [Fact]
        public void GetGraphTopologicSort_ShouldReturnEmptyForEmptyGraph()
        {
            // Arrange
            var emptyGraph = new PsBidirectionalGraph(); // Create an empty graph

            // Add the parameters for Get-GraphTopologicSort
            _powershell.AddCommand("Get-GraphTopologicSort")
                .AddParameter("Graph", emptyGraph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().ContainSingle("because the graph contains a single empty element")
                .Which.Should().BeOfType<PSObject>()
                .Which.BaseObject.Should().BeAssignableTo<IEnumerable<PSVertex>>()
                .Which.Should().BeEmpty("because the single result is an empty collection of vertices");


        }

        [Fact]
        public void GetGraphTopologicSort_ShouldThrowExceptionForCyclicGraph()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph1; // Using a cyclic graph

            // Add the parameters for Get-GraphTopologicSort
            _powershell.AddCommand("Get-GraphTopologicSort")
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>().WithMessage("*The graph contains at least one cycle*");
        }
    }
}
