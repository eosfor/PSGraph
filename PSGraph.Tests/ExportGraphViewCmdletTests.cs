using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using QuikGraph.Graphviz;
using QuikGraph.Serialization;

namespace PSGraph.Tests
{
    public class ExportGraphViewCmdLetTests : IDisposable
    {
        private PowerShell _powershell;
        private string _tempDirectory;

        public ExportGraphViewCmdLetTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.ExportGraphViewCmdLet).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();

            // Create a temporary directory for test files
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            _powershell.Dispose();
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Fact]
        public void ExportGraph_GraphvizFormat_WritesDotFile()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph.dot");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "Graphviz")
                .AddParameter("Path", filePath);

            // Act
            _powershell.Invoke();

            // Assert
            File.Exists(filePath).Should().BeTrue("because the DOT file should be created");

            var content = File.ReadAllText(filePath);
            content.Should().NotBeNullOrWhiteSpace("because the DOT file should contain content");
            content.Should().Contain("digraph", "because Graphviz DOT format should start with 'digraph'");
        }

        [Fact]
        public void ExportGraph_GraphMLFormat_WritesGraphMLFile()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph.graphml");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "GraphML")
                .AddParameter("Path", filePath);

            // Act
            _powershell.Invoke();

            // Assert
            File.Exists(filePath).Should().BeTrue("because the GraphML file should be created");

            var content = File.ReadAllText(filePath);
            content.Should().NotBeNullOrWhiteSpace("because the GraphML file should contain content");
            content.Should().Contain("<graphml", "because GraphML files start with '<graphml'");
        }

        [Fact]
        public void ExportGraph_MSAGL_Sugiyama_WritesSvgFile()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph.svg");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "MSAGL_SUGIYAMA")
                .AddParameter("Path", filePath);

            // Act
            _powershell.Invoke();

            // Assert
            File.Exists(filePath).Should().BeTrue("because the SVG file should be created");

            var content = File.ReadAllText(filePath);
            content.Should().NotBeNullOrWhiteSpace("because the SVG file should contain content");
            content.Should().Contain("<svg", "because SVG files start with '<svg'");
        }

        [Fact]
        public void ExportGraph_NullGraph_ThrowsException()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "graph.dot");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", null)
                .AddParameter("Format", "Graphviz")
                .AddParameter("Path", filePath);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Cannot validate argument on parameter 'Graph'. The argument is null or empty*");
        }

        [Fact]
        public void ExportGraph_InvalidFormat_ThrowsException()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph.output");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "InvalidFormat")
                .AddParameter("Path", filePath);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            // Assuming that the cmdlet defaults to Graphviz format on invalid format
            act.Should().Throw<ParameterBindingException>().WithMessage("*Cannot bind parameter 'Format'. Cannot convert value \"InvalidFormat\"*");
        }

        [Fact]
        public void ExportGraph_NoPathProvided_ReturnsOutput()
        {
            // Arrange
            var graph = CreateSampleGraph();

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "Graphviz");

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty("because output should be returned when Path is not provided");
            var output = results[0].BaseObject as string;
            output.Should().NotBeNullOrWhiteSpace("because the output should be a non-empty string");
            output.Should().Contain("digraph", "because the output should be Graphviz DOT format");
        }

        [Fact]
        public void ExportGraph_InvalidPath_ThrowsException()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var invalidFilePath = Path.Combine(_tempDirectory, "invalid\0file.dot"); // Invalid file name

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "Graphviz")
                .AddParameter("Path", invalidFilePath);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<Exception>().WithMessage("*Null character in path*");
        }

        [Fact]
        public void ExportGraph_MSAGL_MDS_WritesSvgFile()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph_mds.svg");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "MSAGL_MDS")
                .AddParameter("Path", filePath);

            // Act
            _powershell.Invoke();

            // Assert
            File.Exists(filePath).Should().BeTrue("because the SVG file should be created");

            var content = File.ReadAllText(filePath);
            content.Should().NotBeNullOrWhiteSpace("because the SVG file should contain content");
            content.Should().Contain("<svg", "because SVG files start with '<svg'");
        }

        [Fact]
        public void ExportGraph_MSAGL_FastIncremental_WritesSvgFile()
        {
            // Arrange
            var graph = CreateSampleGraph();
            var filePath = Path.Combine(_tempDirectory, "graph_fastincremental.svg");

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", graph)
                .AddParameter("Format", "MSAGL_FASTINCREMENTAL")
                .AddParameter("Path", filePath);

            // Act
            _powershell.Invoke();

            // Assert
            File.Exists(filePath).Should().BeTrue("because the SVG file should be created");

            var content = File.ReadAllText(filePath);
            content.Should().NotBeNullOrWhiteSpace("because the SVG file should contain content");
            content.Should().Contain("<svg", "because SVG files start with '<svg'");
        }

        [Fact]
        public void ExportGraph_InvalidGraph_ThrowsException()
        {
            // Arrange
            var invalidGraph = new object(); // Not a PsBidirectionalGraph

            _powershell.AddCommand("Export-Graph")
                .AddParameter("Graph", invalidGraph)
                .AddParameter("Format", "Graphviz");

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>();
        }

        private PsBidirectionalGraph CreateSampleGraph()
        {
            var graph = new PsBidirectionalGraph();

            var vertexA = new PSVertex("A");
            var vertexB = new PSVertex("B");
            var vertexC = new PSVertex("C");

            graph.AddVertexRange(new[] { vertexA, vertexB, vertexC });

            var edgeAB = new PSEdge(vertexA, vertexB, new PSEdgeTag());
            var edgeBC = new PSEdge(vertexB, vertexC, new PSEdgeTag());

            graph.AddEdgeRange(new[] { edgeAB, edgeBC });

            return graph;
        }
    }
}
