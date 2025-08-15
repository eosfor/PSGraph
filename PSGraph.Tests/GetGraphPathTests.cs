using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PSGraph.Tests
{
    public class GetGraphPathCmdletTests : IDisposable
    {
        private PowerShell _powershell;

        public GetGraphPathCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphPath).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphPath_ShortestPathExists()
        {
            // Arrange
            // Create a new graph
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            // Create vertices
            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");
            var vertexC = new PSVertex("C");
            var vertexD = new PSVertex("D");

            graph.AddVertexRange(new[] { vertexA, vertexB, vertexC, vertexD });

            // Create edges with weights and tags
            var edgeAB = new PSEdge(vertexA, vertexB, new PSEdgeTag()) { Weight = 1 };
            var edgeBC = new PSEdge(vertexB, vertexC, new PSEdgeTag()) { Weight = 1 };
            var edgeAC = new PSEdge(vertexA, vertexC, new PSEdgeTag()) { Weight = 5 };
            var edgeCD = new PSEdge(vertexC, vertexD, new PSEdgeTag()) { Weight = 1 };

            graph.AddEdgeRange(new[] { edgeAB, edgeBC, edgeAC, edgeCD });

            // Add the parameters for Get-GraphPath
            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexD)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var pathEdges = results[0].BaseObject as IEnumerable<PSEdge>;
            pathEdges.Should().NotBeNull();

            var path = pathEdges.ToList();
            // With weights (A->B=1, B->C=1, A->C=5, C->D=1) the true shortest path is A->B->C->D (total 3)
            path.Should().HaveCount(3);

            path[0].Source.Should().Be(vertexA);
            path[0].Target.Should().Be(vertexB);

            path[1].Source.Should().Be(vertexB);
            path[1].Target.Should().Be(vertexC);

            path[2].Source.Should().Be(vertexC);
            path[2].Target.Should().Be(vertexD);
        }


        [Fact]
        public void GetGraphPath_NoPathExists()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");

            graph.AddVertexRange(new[] { vertexA, vertexB });

            // No edges between A and B

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexB)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeEmpty("because there is no path between the vertices");
            _powershell.Streams.Error.Should().BeEmpty("because no exception should be thrown");
        }

        [Fact]
        public void GetGraphPath_InvalidVertices_ThrowsException()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");

            // Note: Vertices are not added to the graph

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexB)
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<CmdletInvocationException>().WithMessage("*Graph does not contain the provided root vertex*");
        }

        [Fact]
        public void GetGraphPath_NullFromVertex_ThrowsException()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexB = new PSVertex("B");
            graph.AddVertex(vertexB);

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", null)
                .AddParameter("To", vertexB)
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Cannot validate argument on parameter 'From'. The argument is null or empty*");
        }

        [Fact]
        public void GetGraphPath_NullToVertex_ThrowsException()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexA = new PSVertex("A");
            graph.AddVertex(vertexA);

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", null)
                .AddParameter("Graph", graph);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Cannot validate argument on parameter 'To'. The argument is null or empty*");
        }

        [Fact]
        public void GetGraphPath_NullGraph_ThrowsException()
        {
            // Arrange
            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexB)
                .AddParameter("Graph", null);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Cannot validate argument on parameter 'Graph'. The argument is null or empty*");
        }

        [Fact]
        public void GetGraphPath_SameFromAndToVertex()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var vertexA = new PSVertex("A");
            graph.AddVertex(vertexA);

            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexA)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeEmpty("because the source and target are the same vertex");
        }

        [Fact]
        public void GetGraphPath_WithEdgeWeights()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            // Create vertices
            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");
            var vertexC = new PSVertex("C");
            var vertexD = new PSVertex("D");

            graph.AddVertexRange(new[] { vertexA, vertexB, vertexC, vertexD });

            // Create edges with different weights and tags
            var edgeAB = new PSEdge(vertexA, vertexB, new PSEdgeTag()) { Weight = 10 };
            var edgeAC = new PSEdge(vertexA, vertexC, new PSEdgeTag()) { Weight = 1 };
            var edgeCB = new PSEdge(vertexC, vertexB, new PSEdgeTag()) { Weight = 1 };
            var edgeCD = new PSEdge(vertexC, vertexD, new PSEdgeTag()) { Weight = 3 };

            graph.AddEdgeRange(new[] { edgeAB, edgeAC, edgeCB, edgeCD });

            // Add the parameters for Get-GraphPath
            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexD)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var pathEdges = results[0].BaseObject as IEnumerable<PSEdge>;
            pathEdges.Should().NotBeNull();

            var path = pathEdges.ToList();
            path.Should().HaveCount(2);

            // Expected path: A -> C -> D
            path[0].Source.Should().Be(vertexA);
            path[0].Target.Should().Be(vertexC);

            path[1].Source.Should().Be(vertexC);
            path[1].Target.Should().Be(vertexD);
        }



        [Fact]
        public void GetGraphPath_DirectedGraph_NoPathExists()
        {
            // Arrange
            // Create a new graph (directed by default)
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            // Create vertices
            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");

            graph.AddVertexRange(new[] { vertexA, vertexB });

            // Create an edge from B to A
            var edgeBA = new PSEdge(vertexB, vertexA, new PSEdgeTag());
            graph.AddEdge(edgeBA);

            // Add the parameters for Get-GraphPath
            _powershell.AddCommand("Get-GraphPath")
                .AddParameter("From", vertexA)
                .AddParameter("To", vertexB)
                .AddParameter("Graph", graph);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().BeNullOrEmpty("because there is no path from A to B in a directed graph");
        }
    }
}
