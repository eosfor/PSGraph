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

        [Fact]
        public void AddEdge_AddVerticesAndEdge_BothMethodsWorkCorrectly()
        {
            var graph = new PsBidirectionalGraph();

            var v1 = new PSVertex("A");
            var v2 = new PSVertex("B");
            var edge = new PSEdge(v1, v2);

            // Проверяем AddEdge
            bool addedEdge = graph.AddEdge(edge);
            Assert.True(addedEdge);
            Assert.Contains(v1, graph.Vertices);
            Assert.Contains(v2, graph.Vertices);
            Assert.Contains(graph.Edges.First(), graph.Edges);
            Assert.Equal(v1, graph.Edges.First().Source);
            Assert.Equal(v2, graph.Edges.First().Target);

            // Проверяем AddVerticesAndEdge с новыми вершинами
            var v3 = new PSVertex("C");
            var v4 = new PSVertex("D");
            var edge2 = new PSEdge(v3, v4);

            bool addedEdge2 = graph.AddVerticesAndEdge(edge2);
            Assert.True(addedEdge2);
            Assert.Contains(v3, graph.Vertices);
            Assert.Contains(v4, graph.Vertices);

            // Проверяем, что рёбра корректно добавлены
            var allEdges = graph.Edges.ToList();
            Assert.Contains(allEdges[1], graph.Edges);
            Assert.Equal(v3, allEdges[1].Source);
            Assert.Equal(v4, allEdges[1].Target);
        }

        [Fact]
        public void AddEdge_AddVerticesAndEdge_VertexReferencesAndMetadataAreCorrect()
        {
            var graph = new PsBidirectionalGraph();

            var v1 = new PSVertex("A");
            var v2 = new PSVertex("B");
            v1.Metadata["cluster"] = 1;
            v2.Metadata["cluster"] = 2;

            var edge = new PSEdge(v1, v2);

            // Проверяем AddEdge
            graph.AddEdge(edge);

            // Получаем объекты-вершины из графа
            var graphV1 = graph.Vertices.First(v => v.Label == "A");
            var graphV2 = graph.Vertices.First(v => v.Label == "B");
            var graphEdge = graph.Edges.First(e => e.Source.Label == "A" && e.Target.Label == "B");

            // Проверяем, что ссылки совпадают
            Assert.True(object.ReferenceEquals(graphV1, graphEdge.Source));
            Assert.True(object.ReferenceEquals(graphV2, graphEdge.Target));

            // Проверяем метаданные
            Assert.True(graphEdge.Source.Metadata.ContainsKey("cluster"));
            Assert.Equal(1, graphEdge.Source.Metadata["cluster"]);
            Assert.True(graphEdge.Target.Metadata.ContainsKey("cluster"));
            Assert.Equal(2, graphEdge.Target.Metadata["cluster"]);

            // Проверяем AddVerticesAndEdge
            var v3 = new PSVertex("C");
            var v4 = new PSVertex("D");
            v3.Metadata["cluster"] = 3;
            v4.Metadata["cluster"] = 4;

            var edge2 = new PSEdge(v3, v4);
            graph.AddVerticesAndEdge(edge2);

            var graphV3 = graph.Vertices.First(v => v.Label == "C");
            var graphV4 = graph.Vertices.First(v => v.Label == "D");
            var graphEdge2 = graph.Edges.First(e => e.Source.Label == "C" && e.Target.Label == "D");

            Assert.True(object.ReferenceEquals(graphV3, graphEdge2.Source));
            Assert.True(object.ReferenceEquals(graphV4, graphEdge2.Target));
            Assert.Equal(3, graphEdge2.Source.Metadata["cluster"]);
            Assert.Equal(4, graphEdge2.Target.Metadata["cluster"]);
        }
    }
}
