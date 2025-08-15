using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using PSGraph.Model;
using PSGraph.DesignStructureMatrix;
using PSGraph.Cmdlets; // reference to locate assembly
using Xunit;

namespace PSGraph.Tests;

public class DsmCmdletsBasicTests : IDisposable
{
    private readonly PowerShell _ps;

    public DsmCmdletsBasicTests()
    {
        _ps = PowerShell.Create();
        // Import module by assembly to expose cmdlets
        _ps.AddCommand("Import-Module").AddParameter("Assembly", typeof(NewDSMCmdlet).Assembly);
        _ps.Invoke();
        _ps.Commands.Clear();
    }

    public void Dispose() => _ps.Dispose();

    private PsBidirectionalGraph BuildGraph(params (string from,string to)[] edges)
    {
        var g = new PsBidirectionalGraph();
        foreach (var (f,t) in edges)
        {
            var vs = g.Vertices.FirstOrDefault(v=>v.Label==f) ?? new PSVertex(f);
            var vt = g.Vertices.FirstOrDefault(v=>v.Label==t) ?? new PSVertex(t);
            g.AddEdge(new PSEdge(vs, vt));
        }
        return g;
    }

    [Fact]
    public void NewDSMCmdlet_BuildsDsmFromGraph()
    {
        var g = BuildGraph(("A","B"),("B","C"));
        _ps.AddCommand("New-DSM").AddParameter("Graph", g);
        var result = _ps.Invoke();
        result.Should().HaveCount(1);
        var dsm = result[0].BaseObject as IDsm;
        dsm.Should().NotBeNull();
        dsm!.RowIndex.Count.Should().Be(3);
        // A->B, B->C edges should have weight 1
        var a = g.Vertices.First(v=>v.Label=="A");
        var b = g.Vertices.First(v=>v.Label=="B");
        var c = g.Vertices.First(v=>v.Label=="C");
        dsm[a,b].Should().Be(1);
        dsm[b,c].Should().Be(1);
    }

    [Fact]
    public void ExportDSMCmdlet_TextFormat_ReturnsStringAndWritesFile()
    {
        var g = BuildGraph(("X","Y"),("Y","Z"));
        var dsm = new DsmClassic(g);

        // Direct string (no Path)
        _ps.AddCommand("Export-DSM").AddParameter("Dsm", dsm).AddParameter("Format", DSMExportTypes.TEXT);
        var r1 = _ps.Invoke();
        r1.Should().HaveCount(1);
    var text = r1[0].BaseObject as string;
    text.Should().NotBeNull();
    // Text export is a plain adjacency matrix (no labels), ensure size (3 lines) and contains a '1'
    var lines = text!.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    lines.Length.Should().Be(3);
    text.Should().Contain("1");
        _ps.Commands.Clear();

        // With Path
        var tmp = Path.Combine(Path.GetTempPath(), $"psgraph_dsm_{Guid.NewGuid():N}.txt");
        _ps.AddCommand("Export-DSM")
            .AddParameter("Dsm", dsm)
            .AddParameter("Format", DSMExportTypes.TEXT)
            .AddParameter("Path", tmp);
        var r2 = _ps.Invoke();
        r2.Should().BeEmpty(); // output redirected to file
        File.Exists(tmp).Should().BeTrue();
        var fileContent = File.ReadAllText(tmp);
    var fileLines = fileContent.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    fileLines.Length.Should().Be(3);
    fileContent.Should().Contain("1");
        File.Delete(tmp);
    }

    [Theory]
    [InlineData(ClusteringAlgorithmOptions.Classic)]
    [InlineData(ClusteringAlgorithmOptions.GraphBased)]
    public void StartDSMClusteringCmdlet_ReturnsPartitioningResult(ClusteringAlgorithmOptions algo)
    {
        // Small graph with a simple cycle to exercise clustering
        var g = BuildGraph(("A","B"),("B","A"),("B","C"),("C","D"));
        var dsm = new DsmClassic(g);
        _ps.AddCommand("Start-DSMClustering")
            .AddParameter("Dsm", dsm)
            .AddParameter("ClusteringAlgorithm", algo);
        var r = _ps.Invoke();
        r.Should().HaveCount(1);
        var pr = r[0].BaseObject as PartitioningResult;
        pr.Should().NotBeNull();
        pr!.Dsm.Should().NotBeNull();
        pr.Algorithm.Should().NotBeNull();
        _ps.Commands.Clear();

        // Detailed variant
        _ps.AddCommand("Start-DSMClustering")
            .AddParameter("Dsm", dsm)
            .AddParameter("ClusteringAlgorithm", algo)
            .AddParameter("Detailed", true);
        var rDetail = _ps.Invoke();
        rDetail.Should().HaveCount(1);
        rDetail[0].BaseObject.Should().NotBeNull(); // extended result object
    }
}
