using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Collections.Generic;

namespace PSGraph.Tests
{
    public class EdgeCmdletsTests : IDisposable
    {
        private PowerShell _powershell;

        public EdgeCmdletsTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetInEdgeCmdlet).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetInEdgesCmdlet_ShouldReturnInEdges()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph5; // Using real graph data
            var vertexD = new PSVertex("D");

            // Add the parameters for Get-InEdges
            _powershell.AddCommand("Get-InEdge")
                .AddParameter("Vertex", vertexD)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var edges = results[0].BaseObject as IEnumerable<PSEdge>;
            edges.Should().NotBeNull();

            // Check the incoming edge to D
            edges.Should().Contain(e => e.Source.ToString() == "A" && e.Target.ToString() == "D");
        }

        [Fact]
        public void GetInEdgesCmdlet_ShouldReturnEmptyForNoInEdges()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph5; // Using real graph data
            var vertexA = new PSVertex("A"); // A has no in-edges

            // Add the parameters for Get-InEdges
            _powershell.AddCommand("Get-InEdge")
                .AddParameter("Vertex", vertexA)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeEmpty("because vertex A has no incoming edges");
        }

        [Fact]
        public void GetOutEdgesCmdlet_ShouldReturnOutEdges()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph5; // Using real graph data
            var vertexA = new PSVertex("A");

            // Add the parameters for Get-OutEdges
            _powershell.AddCommand("Get-OutEdge")
                .AddParameter("Vertex", vertexA)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var edges = results[0].BaseObject as IEnumerable<PSEdge>;
            edges.Should().NotBeNull();

            // Check the outgoing edges from A
            edges.Should().Contain(e => e.Source.ToString() == "A" && e.Target.ToString() == "D");
            edges.Should().Contain(e => e.Source.ToString() == "A" && e.Target.ToString() == "E");
        }

        [Fact]
        public void GetOutEdgesCmdlet_ShouldReturnEmptyForNoOutEdges()
        {
            // Arrange
            var graph = GraphTestData.SimpleTestGraph5; // Using real graph data
            var vertexI = new PSVertex("I"); // I has no out-edges

            // Add the parameters for Get-OutEdges
            _powershell.AddCommand("Get-OutEdge")
                .AddParameter("Vertex", vertexI)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeEmpty("because vertex I has no outgoing edges");
        }
    }
}
