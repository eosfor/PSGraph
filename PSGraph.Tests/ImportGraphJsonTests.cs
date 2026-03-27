using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.IO;
using System.Linq;

namespace PSGraph.Tests
{
    public class ImportGraphJsonTests : IDisposable
    {
        private PowerShell _powershell;
        private string _tempDirectory;

        public ImportGraphJsonTests()
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
        public void ImportJson_WithNodesAndEdges()
        {
            var json = @"{
  ""nodes"": [{ ""id"": ""A"" }, { ""id"": ""B"" }, { ""id"": ""C"" }],
  ""edges"": [
    { ""source"": ""A"", ""target"": ""B"" },
    { ""source"": ""B"", ""target"": ""C"" }
  ]
}";
            var path = WriteTempFile("basic.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            results.Should().NotBeNullOrEmpty();
            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
        }

        [Fact]
        public void ImportJson_EdgesOnly_VerticesCreatedAutomatically()
        {
            var json = @"{
  ""edges"": [
    { ""source"": ""X"", ""target"": ""Y"" },
    { ""source"": ""Y"", ""target"": ""Z"" }
  ]
}";
            var path = WriteTempFile("edgesonly.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
        }

        [Fact]
        public void ImportJson_NodeMetadata()
        {
            var json = @"{
  ""nodes"": [
    { ""id"": ""A"", ""color"": ""red"", ""size"": 10 }
  ],
  ""edges"": []
}";
            var path = WriteTempFile("nodemeta.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            var vertex = graph.Vertices.First(v => v.Label == "A");
            vertex.Metadata.Should().ContainKey("color");
            vertex.Metadata["color"].Should().Be("red");
            vertex.Metadata.Should().ContainKey("size");
            vertex.Metadata["size"].Should().Be(10L);
        }

        [Fact]
        public void ImportJson_EdgeLabelAndWeight()
        {
            var json = @"{
  ""edges"": [
    { ""source"": ""A"", ""target"": ""B"", ""label"": ""link"", ""weight"": 5 }
  ]
}";
            var path = WriteTempFile("edgemeta.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            var edge = graph.Edges.First();
            edge.Label.Should().Be("link");
            edge.Weight.Should().Be(5);
        }

        [Fact]
        public void ImportJson_EdgeExtraPropertiesToRenderProperties()
        {
            var json = @"{
  ""edges"": [
    { ""source"": ""A"", ""target"": ""B"", ""style"": ""dashed"", ""priority"": true }
  ]
}";
            var path = WriteTempFile("edgeextra.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            var edge = graph.Edges.First();
            edge.RenderProperties.Should().ContainKey("style");
            edge.RenderProperties["style"].Should().Be("dashed");
            edge.RenderProperties.Should().ContainKey("priority");
            edge.RenderProperties["priority"].Should().Be(true);
        }

        [Fact]
        public void ImportJson_EmptyGraph()
        {
            var json = @"{ ""nodes"": [], ""edges"": [] }";
            var path = WriteTempFile("empty.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(0);
            graph.EdgeCount.Should().Be(0);
        }

        [Fact]
        public void ImportJson_InvalidJson_ThrowsException()
        {
            var json = "{ not valid json }}}";
            var path = WriteTempFile("invalid.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>();
        }

        [Fact]
        public void ImportJson_MissingSourceTarget_ThrowsException()
        {
            var json = @"{ ""edges"": [{ ""name"": ""oops"" }] }";
            var path = WriteTempFile("nosource.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>().WithMessage("*source*");
        }

        [Fact]
        public void ImportJson_NodeWithLabelProperty()
        {
            var json = @"{
  ""nodes"": [{ ""label"": ""MyNode"" }],
  ""edges"": []
}";
            var path = WriteTempFile("labelprop.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(1);
            graph.Vertices.First().Label.Should().Be("MyNode");
        }

        [Fact]
        public void ImportJson_FileNotFound_ThrowsException()
        {
            var path = Path.Combine(_tempDirectory, "nonexistent.json");

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            Action act = () => _powershell.Invoke();
            act.Should().Throw<CmdletInvocationException>().WithMessage("*not found*");
        }

        [Fact]
        public void ImportJson_VertexDeduplication()
        {
            var json = @"{
  ""nodes"": [{ ""id"": ""A"" }, { ""id"": ""B"" }],
  ""edges"": [
    { ""source"": ""A"", ""target"": ""B"" },
    { ""source"": ""A"", ""target"": ""B"" }
  ]
}";
            var path = WriteTempFile("dedup.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(2);
            // PsBidirectionalGraph disallows parallel edges by default
            graph.EdgeCount.Should().Be(1);
        }

        [Fact]
        public void ImportJson_LinksAliasForEdges()
        {
            var json = @"{
  ""nodes"": [{ ""id"": ""A"" }, { ""id"": ""B"" }],
  ""links"": [
    { ""source"": ""A"", ""target"": ""B"" }
  ]
}";
            var path = WriteTempFile("links.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(2);
            graph.EdgeCount.Should().Be(1);
        }

        [Fact]
        public void ImportJson_NodeWithNameProperty()
        {
            var json = @"{
  ""nodes"": [{ ""name"": ""Alice"" }, { ""name"": ""Bob"" }],
  ""edges"": [{ ""source"": ""Alice"", ""target"": ""Bob"" }]
}";
            var path = WriteTempFile("names.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(2);
            graph.Vertices.Should().Contain(v => v.Label == "Alice");
            graph.Vertices.Should().Contain(v => v.Label == "Bob");
            graph.EdgeCount.Should().Be(1);
        }

        [Fact]
        public void ImportJson_NumericIndexReferences()
        {
            var json = @"{
  ""nodes"": [{ ""id"": ""X"" }, { ""id"": ""Y"" }, { ""id"": ""Z"" }],
  ""links"": [
    { ""source"": 0, ""target"": 1 },
    { ""source"": 1, ""target"": 2 }
  ]
}";
            var path = WriteTempFile("numeric.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);
            graph.Edges.Should().Contain(e => e.Source.Label == "X" && e.Target.Label == "Y");
            graph.Edges.Should().Contain(e => e.Source.Label == "Y" && e.Target.Label == "Z");
        }

        [Fact]
        public void ImportJson_D3LesMiserablesStyle()
        {
            // D3.js Les Miserables format: "name" for nodes, "links" with numeric indices, "group"/"value" metadata
            var json = @"{
  ""nodes"": [
    { ""name"": ""Myriel"", ""group"": 1 },
    { ""name"": ""Napoleon"", ""group"": 1 },
    { ""name"": ""Valjean"", ""group"": 2 }
  ],
  ""links"": [
    { ""source"": 0, ""target"": 1, ""value"": 1 },
    { ""source"": 0, ""target"": 2, ""value"": 8 }
  ]
}";
            var path = WriteTempFile("lesmis.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            graph.VertexCount.Should().Be(3);
            graph.EdgeCount.Should().Be(2);

            var myriel = graph.Vertices.First(v => v.Label == "Myriel");
            myriel.Metadata.Should().ContainKey("group");
            myriel.Metadata["group"].Should().Be(1L);

            // "value" goes to RenderProperties on edges
            var edge = graph.Edges.First(e => e.Source.Label == "Myriel" && e.Target.Label == "Valjean");
            edge.RenderProperties.Should().ContainKey("value");
        }

        [Fact]
        public void ImportJson_NumericIndexOutOfRange_FallsBackToStringLabel()
        {
            var json = @"{
  ""nodes"": [{ ""id"": ""A"" }],
  ""links"": [{ ""source"": 0, ""target"": 5 }]
}";
            var path = WriteTempFile("outofrange.json", json);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", path)
                .AddParameter("Format", GraphImportTypes.Json);

            var results = _powershell.Invoke();

            var graph = (PsBidirectionalGraph)results[0].BaseObject;
            // Index 5 is out of range, so "5" becomes a string vertex label
            graph.VertexCount.Should().Be(2);
            graph.Vertices.Should().Contain(v => v.Label == "A");
            graph.Vertices.Should().Contain(v => v.Label == "5");
        }

        private string WriteTempFile(string name, string content)
        {
            var path = Path.Combine(_tempDirectory, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
