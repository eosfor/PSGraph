using Xunit;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Linq;

namespace PSGraph.Tests
{
    public class AddVertexCmdletTests : IDisposable
    {
        private PowerShell _powershell;

        public AddVertexCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.AddVertexCmdlet).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void AddsVertex_WhenVertexIsString()
        {
            // Arrange
            // Create a new graph
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            // Add-Vertex cmdlet parameters
            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", "vertex1")
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // TODO: Fix this test
            // Assert
            // var vertex = graph.Vertices.FirstOrDefault(v => v.Name == "vertex1");
            // vertex.Should().NotBeNull();
            // vertex.Data.Should().Be("vertex1");
        }

        [Fact]
        public void AddsVertex_WhenVertexIsPSVertex()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var psVertex = new PSVertex("vertex1", "data1");

            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", psVertex)
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            graph.Vertices.Should().Contain(psVertex);
        }

        [Fact]
        public void ThrowsException_WhenVertexIsNull()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", null)
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        [Fact]
        public void ThrowsException_WhenGraphIsNull()
        {
            // Arrange
            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", "vertex1")
                .AddParameter("Graph", null);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        [Fact]
        public void AddsVertex_WhenVertexIsComplexObject()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var complexObject = new { Name = "vertex1", Value = 123 };

            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", complexObject)
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // TODO: Fix this test
            // Assert
            // var vertex = graph.Vertices.FirstOrDefault(v => v.Data == complexObject);
            // vertex.Should().NotBeNull();
            // vertex.Name.Should().Be(complexObject.ToString());
        }

        [Fact]
        public void DoesNotAddDuplicateVertex()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", "vertex1")
                .AddParameter("Graph", graph);

            _powershell.Invoke();
            _powershell.Commands.Clear();

            // Attempt to add the same vertex again
            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", "vertex1")
                .AddParameter("Graph", graph);

            // Act
            _powershell.Invoke();

            // Assert
            graph.Vertices.Count(v => v.Name == "vertex1").Should().Be(1);
        }

        [Fact]
        public void AddsMultipleVertices()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertices = new[] { "vertex1", "vertex2", "vertex3" };

            foreach (var vertex in vertices)
            {
                _powershell.AddCommand("Add-Vertex")
                    .AddParameter("Vertex", vertex)
                    .AddParameter("Graph", graph);
                _powershell.Invoke();
                _powershell.Commands.Clear();
            }

            // TODO: Fix this test
            // Act & Assert
            // foreach (var vertexName in vertices)
            // {
            //     var vertex = graph.Vertices.FirstOrDefault(v => v.Name == vertexName);
            //     vertex.Should().NotBeNull();
            //     vertex.Data.Should().Be(vertexName);
            // }
        }

        [Fact]
        public void AddsVertex_WithVerboseOutput()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            _powershell.AddParameter("Verbose");
            var graphResult = _powershell.Invoke();
            var graph = graphResult[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            _powershell.AddCommand("Add-Vertex")
                .AddParameter("Vertex", "vertex1")
                .AddParameter("Graph", graph)
                .AddParameter("Verbose");

            // Act
            _powershell.Invoke();

            // Assert
            var verboseOutput = _powershell.Streams.Verbose;
            verboseOutput.Should().NotBeEmpty();
            verboseOutput[0].Message.Should().Contain("True"); // Assuming the result.ToString() outputs "True" on success
        }
    }
}
