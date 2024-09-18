using Xunit;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Linq;

namespace PSGraph.Tests
{
    public class AddEdgeCmdLetTests : IDisposable
    {
        private PowerShell _powershell;

        public AddEdgeCmdLetTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(PSGraph.Cmdlets.AddEdgeCmdLet)));
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void AddsEdgeBetweenExistingVertices()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertex1 = new PSVertex("vertex1", "data1");
            var vertex2 = new PSVertex("vertex2", "data2");
            graph.AddVertex(vertex1);
            graph.AddVertex(vertex2);

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", vertex1)
                .AddParameter("To", vertex2)
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            graph.ContainsEdge(vertex1, vertex2).Should().BeTrue();
        }

        [Fact]
        public void AddsEdgeAndCreatesNewVertices()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            var vertex1 = graph.Vertices.FirstOrDefault(v => v.Name == "vertex1");
            var vertex2 = graph.Vertices.FirstOrDefault(v => v.Name == "vertex2");

            vertex1.Should().NotBeNull();
            vertex2.Should().NotBeNull();
            graph.ContainsEdge(vertex1, vertex2).Should().BeTrue();
        }

        [Fact]
        public void AddsEdgeWithTag()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph)
                .AddParameter("Tag", "edgeTag");

            // Act
            _powershell.Invoke();

            // Assert
            var edge = graph.Edges.FirstOrDefault(e => e.Source.Name == "vertex1" && e.Target.Name == "vertex2");
            edge.Should().NotBeNull();
            //edge.Tag.Name.Should().Be("edgeTag");
        }

        [Fact]
        public void ThrowsExceptionWhenFromIsNull()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", null)
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        [Fact]
        public void ThrowsExceptionWhenToIsNull()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", null)
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        [Fact]
        public void ThrowsExceptionWhenGraphIsNull()
        {
            // Arrange
            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", null);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        [Fact]
        public void AddsEdgeBetweenPSVertexAndString()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertex1 = new PSVertex("vertex1", "data1");
            graph.AddVertex(vertex1);

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", vertex1)
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            var vertex2 = graph.Vertices.FirstOrDefault(v => v.Name == "vertex2");
            vertex2.Should().NotBeNull();
            graph.ContainsEdge(vertex1, vertex2).Should().BeTrue();
        }

        [Fact]
        public void AddsEdgeBetweenStringAndPSVertex()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertex2 = new PSVertex("vertex2", "data2");
            graph.AddVertex(vertex2);

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", vertex2)
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            var vertex1 = graph.Vertices.FirstOrDefault(v => v.Name == "vertex1");
            vertex1.Should().NotBeNull();
            graph.ContainsEdge(vertex1, vertex2).Should().BeTrue();
        }

        [Fact]
        public void AddsEdgeWithComplexObjectData()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var complexObject = new { Name = "vertex1", Value = 123 };

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", complexObject)
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // TODO: Fix this test
            // Assert
            // var vertex1 = graph.Vertices.FirstOrDefault(v => v.Data == complexObject);
            // var vertex2 = graph.Vertices.FirstOrDefault(v => v.Name == "vertex2");
            // vertex1.Should().NotBeNull();
            // vertex2.Should().NotBeNull();
            // graph.ContainsEdge(vertex1, vertex2).Should().BeTrue();
        }

        [Fact]
        public void AddsEdgeWhenTagIsNull()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Edge")
                .AddParameter("From", "vertex1")
                .AddParameter("To", "vertex2")
                .AddParameter("Graph", graph)
                .AddParameter("Tag", null);

            // Act
            _powershell.Invoke();

            // Assert
            var edge = graph.Edges.FirstOrDefault(e => e.Source.Name == "vertex1" && e.Target.Name == "vertex2");
            edge.Should().NotBeNull();
            //edge.Tag.Name.Should().BeNull();
        }
    }
}
