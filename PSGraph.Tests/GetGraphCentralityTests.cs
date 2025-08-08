using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Linq;
using PSGraph.Common.Model;

namespace PSGraph.Tests
{
    public class GetGraphCentralityCmdletTests : IDisposable
    {
        private readonly PowerShell _powershell;

        public GetGraphCentralityCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module").AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphCentrality).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphCentrality_ComputesBetweenness()
        {
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;
            _powershell.Commands.Clear();

            var a = new PSVertex("A");
            var b = new PSVertex("B");
            var c = new PSVertex("C");
            graph.AddVertexRange(new[] { a, b, c });
            graph.AddEdge(new PSEdge(a, b, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(b, c, new PSEdgeTag()));

            _powershell.AddCommand("Get-GraphCentrality").AddParameter("Graph", graph);
            var results = _powershell.Invoke();
            var records = results.Select(r => r.BaseObject as PSCentralityRecord).ToList();

            records.Should().HaveCount(3);
            records.Single(r => r.Vertex == b).Centrality.Should().BeGreaterThan(records.Single(r => r.Vertex == a).Centrality);
            records.Single(r => r.Vertex == b).Centrality.Should().BeGreaterThan(records.Single(r => r.Vertex == c).Centrality);
        }
    }
}
