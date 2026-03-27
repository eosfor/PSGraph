using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class ImportGraphMatrixMarketTests : IDisposable
{
    private readonly PowerShell _powershell;
    private readonly string _tempDirectory;

    public ImportGraphMatrixMarketTests()
    {
        _powershell = PowerShell.Create();
        _powershell.AddCommand("Import-Module")
            .AddParameter("Assembly", typeof(PSGraph.Cmdlets.ImportGraphCmdlet).Assembly);
        _powershell.Invoke();
        _powershell.Commands.Clear();

        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        _powershell.Dispose();
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public void ImportMatrixMarket_PatternSymmetric_ImportsKarateStyleAdjacency()
    {
        var matrixMarket = """
            %%MatrixMarket matrix coordinate pattern symmetric
            % Zachary karate club style sample
            4 4 3
            2 1
            3 1
            4 2
            """;
        var path = WriteTempFile("karate-sample.mtx", matrixMarket);

        _powershell.AddCommand("Import-Graph")
            .AddParameter("Path", path)
            .AddParameter("Format", GraphImportTypes.MatrixMarket);

        var results = _powershell.Invoke();

        results.Should().NotBeNullOrEmpty();
        var graph = (PsBidirectionalGraph)results[0].BaseObject;
        graph.VertexCount.Should().Be(4);
        graph.EdgeCount.Should().Be(3);
        graph.Edges.Should().Contain(e => e.Source.Label == "2" && e.Target.Label == "1");
        graph.Edges.Should().Contain(e => e.Source.Label == "3" && e.Target.Label == "1");
        graph.Edges.Should().Contain(e => e.Source.Label == "4" && e.Target.Label == "2");
    }

    [Fact]
    public void ImportMatrixMarket_IntegerGeneral_PreservesWeightsAndValueMetadata()
    {
        var matrixMarket = """
            %%MatrixMarket matrix coordinate integer general
            3 3 2
            1 2 5
            2 3 7
            """;
        var path = WriteTempFile("weighted.mtx", matrixMarket);

        _powershell.AddCommand("Import-Graph")
            .AddParameter("Path", path)
            .AddParameter("Format", GraphImportTypes.MatrixMarket);

        var results = _powershell.Invoke();

        var graph = (PsBidirectionalGraph)results[0].BaseObject;
        graph.VertexCount.Should().Be(3);
        graph.EdgeCount.Should().Be(2);

        var edge = graph.Edges.First(e => e.Source.Label == "1" && e.Target.Label == "2");
        edge.Weight.Should().Be(5);
        edge.RenderProperties.Should().ContainKey("Value");
        edge.RenderProperties["Value"].Should().Be(5d);
    }

    [Fact]
    public void ImportMatrixMarket_RectangularMatrix_ThrowsException()
    {
        var matrixMarket = """
            %%MatrixMarket matrix coordinate pattern general
            2 3 1
            1 2
            """;
        var path = WriteTempFile("rectangular.mtx", matrixMarket);

        _powershell.AddCommand("Import-Graph")
            .AddParameter("Path", path)
            .AddParameter("Format", GraphImportTypes.MatrixMarket);

        Action act = () => _powershell.Invoke();
        act.Should().Throw<CmdletInvocationException>().WithMessage("*square adjacency matrix*");
    }

    [Fact]
    public void ImportMatrixMarket_ArrayFormat_ThrowsException()
    {
        var matrixMarket = """
            %%MatrixMarket matrix array real general
            2 2
            1
            0
            0
            1
            """;
        var path = WriteTempFile("array.mtx", matrixMarket);

        _powershell.AddCommand("Import-Graph")
            .AddParameter("Path", path)
            .AddParameter("Format", GraphImportTypes.MatrixMarket);

        Action act = () => _powershell.Invoke();
        act.Should().Throw<CmdletInvocationException>().WithMessage("*Only coordinate format is supported*");
    }

    private string WriteTempFile(string name, string content)
    {
        var path = Path.Combine(_tempDirectory, name);
        File.WriteAllText(path, content);
        return path;
    }
}
