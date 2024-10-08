using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using PSGraph.Common.Model;

namespace PSGraph.Tests
{
    public class GetGraphDistanceVectorCmdletTests : IDisposable
    {
        private PowerShell _powershell;

        public GetGraphDistanceVectorCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphDistanceVector).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphDistanceVector_ValidAcyclicGraph_ReturnsDistances()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");
            var vertexC = new PSVertex("C");

            graph.AddVertexRange(new[] { vertexA, vertexB, vertexC });
            graph.AddEdge(new PSEdge(vertexA, vertexB, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(vertexB, vertexC, new PSEdgeTag()));

            _powershell.AddCommand("Get-GraphDistanceVector")
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            
            var distances = results.Select(r => r.BaseObject as PSDistanceVectorRecord).ToList();
            distances.Should().NotBeNull();
            distances.Should().Contain(d => d.Vertex == vertexA && d.Level == 0);
            distances.Should().Contain(d => d.Vertex == vertexB && d.Level == 1);
            distances.Should().Contain(d => d.Vertex == vertexC && d.Level == 2);
        }

        [Fact]
        public void GetGraphDistanceVector_EmptyGraph_ReturnsEmptyDistances()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Get-GraphDistanceVector")
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeEmpty("because the graph is empty and has no vertices to calculate distances");
        }
    }
}