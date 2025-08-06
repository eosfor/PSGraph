using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Linq;
using PSGraph.Common.Model;

namespace PSGraph.Tests
{
    public class GetGraphConnectedComponentCmdletTests : IDisposable
    {
        private readonly PowerShell _powershell;

        public GetGraphConnectedComponentCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module").AddParameter("Assembly", typeof(PSGraph.Cmdlets.GetGraphConnectedComponent).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void GetGraphConnectedComponent_ReturnsComponents()
        {
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;

            _powershell.Commands.Clear();

            var a = new PSVertex("A");
            var b = new PSVertex("B");
            var c = new PSVertex("C");
            var d = new PSVertex("D");
            graph.AddVertexRange(new[] { a, b, c, d });
            graph.AddEdge(new PSEdge(a, b, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(b, a, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(c, d, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(d, c, new PSEdgeTag()));

            _powershell.AddCommand("Get-GraphConnectedComponent").AddParameter("Graph", graph);
            var results = _powershell.Invoke();

            var records = results.Select(r => r.BaseObject as PSConnectedComponentRecord).ToList();
            records.Should().HaveCount(4);
            var compAB = records.Where(r => r.Vertex == a || r.Vertex == b).Select(r => r.ComponentId).Distinct().Single();
            var compCD = records.Where(r => r.Vertex == c || r.Vertex == d).Select(r => r.ComponentId).Distinct().Single();
            compAB.Should().NotBe(compCD);
        }
    }
}
