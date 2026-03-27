using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.IO;
using System.Linq;

namespace PSGraph.Tests
{
    public class ImportGraphCsvTests : IDisposable
    {
        private PowerShell _powershell;
        private string _tempDirectory;

        public ImportGraphCsvTests()
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
        public void ImportCsv_BasicEdgeList()
        {
            var csv = "From,To\nA,B\nB,C\n";
            var path = WriteTempFile("basic.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            results.Should().NotBeNullOrEmpty();
            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
        }

        [Fact]
        public void ImportCsv_WithLabelAndWeight()
        {
            var csv = "From,To,Label,Weight\nA,B,connects,5\nB,C,links,3\n";
            var path = WriteTempFile("labeled.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.EdgeCount.Should().Be(2);

            var ab = graph.Edges.First(e => e.Source.Label == "A" && e.Target.Label == "B");
            ab.Label.Should().Be("connects");
            ab.Weight.Should().Be(5);

            var bc = graph.Edges.First(e => e.Source.Label == "B" && e.Target.Label == "C");
            bc.Label.Should().Be("links");
            bc.Weight.Should().Be(3);
        }

        [Fact]
        public void ImportCsv_ExtraColumnsGoToRenderProperties()
        {
            var csv = "From,To,Color,Priority\nA,B,red,high\n";
            var path = WriteTempFile("meta.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            var edge = graph.Edges.First();
            edge.RenderProperties.Should().ContainKey("Color");
            edge.RenderProperties["Color"].Should().Be("red");
            edge.RenderProperties.Should().ContainKey("Priority");
            edge.RenderProperties["Priority"].Should().Be("high");
        }

        [Fact]
        public void ImportCsv_CustomColumnNames()
        {
            var csv = "Source,Target\nX,Y\nY,Z\n";
            var path = WriteTempFile("custom.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv)
                .AddParameter("FromColumn", "Source")
                .AddParameter("ToColumn", "Target");

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
            graph.Vertices.Should().Contain(v => v.Label == "X");
        }

        [Fact]
        public void ImportCsv_EmptyCsv_HeaderOnly_ProducesEmptyGraph()
        {
            var csv = "From,To\n";
            var path = WriteTempFile("empty.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(0);
            graph.EdgeCount.Should().Be(0);
        }

        [Fact]
        public void ImportCsv_MissingRequiredColumn_ThrowsException()
        {
            var csv = "Name,Value\nA,1\n";
            var path = WriteTempFile("nocol.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>().WithMessage("*'From' not found*");
        }

        [Fact]
        public void ImportCsv_EmptyFile_ThrowsException()
        {
            var path = WriteTempFile("blank.csv", string.Empty);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>().WithMessage("*empty*");
        }

        [Fact]
        public void ImportCsv_QuotedFields()
        {
            var csv = "From,To,Label\n\"Node A\",\"Node B\",\"has, comma\"\n";
            var path = WriteTempFile("quoted.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(2);
            graph.Vertices.Should().Contain(v => v.Label == "Node A");
            graph.Vertices.Should().Contain(v => v.Label == "Node B");

            var edge = graph.Edges.First();
            edge.Label.Should().Be("has, comma");
        }

        [Fact]
        public void ImportCsv_VertexDeduplication()
        {
            var csv = "From,To\nA,B\nA,C\nB,C\n";
            var path = WriteTempFile("dedup.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(3);
        }

        [Fact]
        public void ImportCsv_FileNotFound_ThrowsException()
        {
            var path = Path.Combine(_tempDirectory, "nonexistent.csv");

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>().WithMessage("*not found*");
        }

        [Fact]
        public void ImportCsv_TabDelimiter()
        {
            var tsv = "From\tTo\nA\tB\nB\tC\n";
            var path = WriteTempFile("tab.tsv", tsv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv)
                .AddParameter("Delimiter", '\t');

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
        }

        [Fact]
        public void ImportCsv_CommentLinesSkipped()
        {
            var csv = "# This is a comment\n# Another comment\nFrom,To\nA,B\n";
            var path = WriteTempFile("comments.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(2);
            graph.EdgeCount.Should().Be(1);
        }

        [Fact]
        public void ImportCsv_NoHeader_PositionalColumns()
        {
            // SNAP-style: no header, tab-separated, comment lines
            var tsv = "# Directed graph\n# Nodes: 3 Edges: 2\n# FromNodeId\tToNodeId\n30\t1412\n30\t3352\n";
            var path = WriteTempFile("snap.txt", tsv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv)
                .AddParameter("Delimiter", '\t')
                .AddParameter("NoHeader", true);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
            graph.Vertices.Should().Contain(v => v.Label == "30");
            graph.Vertices.Should().Contain(v => v.Label == "1412");
            graph.Vertices.Should().Contain(v => v.Label == "3352");
        }

        [Fact]
        public void ImportCsv_NoHeader_CommaDelimited()
        {
            // Bitcoin Alpha style: no header, comma-separated, 4 columns
            var csv = "7188,1,10,1407470400\n430,1,10,1376539200\n";
            var path = WriteTempFile("bitcoin.csv", csv);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Csv)
                .AddParameter("NoHeader", true);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3); // 7188, 430, 1
            graph.EdgeCount.Should().Be(2);
        }

        private string WriteTempFile(string name, string content)
        {
            var path = Path.Combine(_tempDirectory, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
