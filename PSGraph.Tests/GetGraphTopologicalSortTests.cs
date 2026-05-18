using System;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests
{
    public class GetGraphTopologicalSortCmdletTests : IDisposable
    {
        private readonly PowerShell _powershell;

        public GetGraphTopologicalSortCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphTopologicalSortCmdlet).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphTopologicalSort_Dag_ReturnsAllVerticesInTopologicalOrder()
        {
            var graph = CreateDiamondGraph();

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph);

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().HaveCount(graph.VertexCount);
            vertices.Should().OnlyContain(vertex => vertex != null);
            AssertTopologicalOrder(graph, vertices!);
        }

        [Fact]
        public void GetGraphTopologicalSort_EmptyGraph_ReturnsEmpty()
        {
            var graph = new PsBidirectionalGraph();

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph);

            var results = _powershell.Invoke();

            results.Should().BeEmpty();
        }

        [Fact]
        public void GetGraphTopologicalSort_CyclicGraph_Throws()
        {
            var graph = new PsBidirectionalGraph();
            graph.AddVerticesAndEdge(new PSEdge(new PSVertex("A"), new PSVertex("B")));
            graph.AddVerticesAndEdge(new PSEdge(new PSVertex("B"), new PSVertex("A")));

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph);

            Action act = () => _powershell.Invoke();

            act.Should().Throw<CmdletInvocationException>()
                .WithMessage("*Topological sort requires a directed acyclic graph*");
        }

        [Fact]
        public void GetGraphTopologicalSort_Reverse_ReturnsTargetsBeforeSources()
        {
            var graph = CreateDiamondGraph();

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph)
                .AddParameter("Reverse");

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().HaveCount(graph.VertexCount);
            vertices.Should().OnlyContain(vertex => vertex != null);
            AssertReverseTopologicalOrder(graph, vertices!);
        }

        [Fact]
        public void GetGraphTopologicalSort_NonUniqueLabels_ReturnsDistinctVertices()
        {
            var graph = new PsBidirectionalGraph(useNonUniqueLabels: true);
            var input = graph.AddOrGetVertex(new PSVertex("x"));
            var plus1 = graph.AddOrGetVertex(new PSVertex("+"));
            var plus2 = graph.AddOrGetVertex(new PSVertex("+"));
            var output = graph.AddOrGetVertex(new PSVertex("out"));

            graph.AddEdge(new PSEdge(input, plus1));
            graph.AddEdge(new PSEdge(input, plus2));
            graph.AddEdge(new PSEdge(plus1, output));
            graph.AddEdge(new PSEdge(plus2, output));

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph);

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().HaveCount(4);
            vertices.Count(vertex => vertex!.Label == "+").Should().Be(2);
            vertices.Should().Contain(vertex => ReferenceEquals(vertex, plus1));
            vertices.Should().Contain(vertex => ReferenceEquals(vertex, plus2));
            AssertTopologicalOrder(graph, vertices!);
        }

        [Fact]
        public void GetGraphTopologicalSort_StartVertex_ReturnsReachableVerticesInTopologicalOrder()
        {
            var graph = CreateDiamondGraph();
            var x = new PSVertex("X");
            var y = new PSVertex("Y");
            graph.AddVerticesAndEdge(new PSEdge(x, y));

            var startVertex = graph.Vertices.Single(vertex => vertex.Label == "B");

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph)
                .AddParameter("StartVertex", startVertex);

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().Equal(
                startVertex,
                graph.Vertices.Single(vertex => vertex.Label == "D"));
            AssertTopologicalOrder(graph, vertices!);
        }

        [Fact]
        public void GetGraphTopologicalSort_StartVertexReverse_ReturnsReachableVerticesInReverseTopologicalOrder()
        {
            var graph = CreateDiamondGraph();
            var startVertex = graph.Vertices.Single(vertex => vertex.Label == "A");

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph)
                .AddParameter("StartVertex", startVertex)
                .AddParameter("Reverse");

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().HaveCount(4);
            vertices.Last().Should().Be(startVertex);
            AssertReverseTopologicalOrder(graph, vertices!);
        }

        [Fact]
        public void GetGraphTopologicalSort_StartVertex_ThrowsWhenVertexIsNotInGraph()
        {
            var graph = CreateDiamondGraph();
            var startVertex = new PSVertex("X");

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph)
                .AddParameter("StartVertex", startVertex);

            Action act = () => _powershell.Invoke();

            act.Should().Throw<CmdletInvocationException>()
                .WithMessage("*The graph does not contain the provided start vertex*");
        }

        [Fact]
        public void GetGraphTopologicalSort_StartVertex_IgnoresUnreachableCycle()
        {
            var graph = CreateDiamondGraph();
            var x = new PSVertex("X");
            var y = new PSVertex("Y");
            graph.AddVerticesAndEdge(new PSEdge(x, y));
            graph.AddVerticesAndEdge(new PSEdge(y, x));

            var startVertex = graph.Vertices.Single(vertex => vertex.Label == "A");

            _powershell.AddCommand("Get-GraphTopologicalSort")
                .AddParameter("Graph", graph)
                .AddParameter("StartVertex", startVertex);

            var results = _powershell.Invoke();

            var vertices = results.Select(result => result.BaseObject as PSVertex).ToList();
            vertices.Should().HaveCount(4);
            vertices.Should().NotContain(vertex => vertex!.Label == "X");
            vertices.Should().NotContain(vertex => vertex!.Label == "Y");
            AssertTopologicalOrder(graph, vertices!);
        }

        private static PsBidirectionalGraph CreateDiamondGraph()
        {
            var graph = new PsBidirectionalGraph();
            var a = new PSVertex("A");
            var b = new PSVertex("B");
            var c = new PSVertex("C");
            var d = new PSVertex("D");

            graph.AddVerticesAndEdge(new PSEdge(a, b));
            graph.AddVerticesAndEdge(new PSEdge(a, c));
            graph.AddVerticesAndEdge(new PSEdge(b, d));
            graph.AddVerticesAndEdge(new PSEdge(c, d));

            return graph;
        }

        private static void AssertTopologicalOrder(PsBidirectionalGraph graph, IList<PSVertex> vertices)
        {
            var index = vertices.Select((vertex, i) => new { vertex, i })
                .ToDictionary(item => item.vertex, item => item.i);

            foreach (var edge in graph.Edges)
            {
                if (!index.ContainsKey(edge.Source) || !index.ContainsKey(edge.Target))
                {
                    continue;
                }

                index[edge.Source].Should().BeLessThan(index[edge.Target]);
            }
        }

        private static void AssertReverseTopologicalOrder(PsBidirectionalGraph graph, IList<PSVertex> vertices)
        {
            var index = vertices.Select((vertex, i) => new { vertex, i })
                .ToDictionary(item => item.vertex, item => item.i);

            foreach (var edge in graph.Edges)
            {
                if (!index.ContainsKey(edge.Source) || !index.ContainsKey(edge.Target))
                {
                    continue;
                }

                index[edge.Target].Should().BeLessThan(index[edge.Source]);
            }
        }
    }
}
