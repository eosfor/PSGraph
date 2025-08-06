using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Linq;
using PSGraph.Common.Model;

namespace PSGraph.Tests
{
    public class FindGraphCycleCmdletTests : IDisposable
    {
        private readonly PowerShell _powershell;

        public FindGraphCycleCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module").AddParameter("Assembly", typeof(PSGraph.Cmdlets.FindGraphCycle).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void FindGraphCycle_DetectsCycle()
        {
            _powershell.AddCommand("New-Graph");
            var graphResults = _powershell.Invoke();
            var graph = graphResults[0].BaseObject as PsBidirectionalGraph;
            _powershell.Commands.Clear();

            var a = new PSVertex("A");
            var b = new PSVertex("B");
            var cVertex = new PSVertex("C");
            var d = new PSVertex("D");
            graph.AddVertexRange(new[] { a, b, cVertex, d });
            graph.AddEdge(new PSEdge(a, b, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(b, cVertex, new PSEdgeTag()));
            graph.AddEdge(new PSEdge(cVertex, a, new PSEdgeTag()));

            _powershell.AddCommand("Find-GraphCycle").AddParameter("Graph", graph);
            var results = _powershell.Invoke();
            var cycles = results.Select(r => r.BaseObject as PSCycleRecord).ToList();
            cycles.Should().NotBeEmpty();
            cycles.Any(cycle => cycle.Vertices.Contains(a) && cycle.Vertices.Contains(b) && cycle.Vertices.Contains(cVertex)).Should().BeTrue();
        }
    }
}
