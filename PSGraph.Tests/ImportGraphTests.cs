using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;

namespace PSGraph.Tests
{
    public class ImportGraphCmdletTests : IDisposable
    {
        private PowerShell _powershell;
        private string _tempDirectory;

        public ImportGraphCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.ImportGraphCmdlet).Assembly);
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
        public void ImportGraph_Success()
        {
            // Arrange
            var graphmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<graphml xmlns='http://graphml.graphdrawing.org/xmlns'
    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
    xsi:schemaLocation='http://graphml.graphdrawing.org/xmlns
     http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd'>
  <graph id='G' edgedefault='directed'>
    <node id='n0'/>
    <node id='n1'/>
    <edge id='e0' source='n0' target='n1'/>
  </graph>
</graphml>";

            var filePath = Path.Combine(_tempDirectory, "test.graphml");
            File.WriteAllText(filePath, graphmlContent);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", filePath);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var graph = results[0].BaseObject as PsBidirectionalGraph;
            graph.Should().NotBeNull();
            graph.VertexCount.Should().Be(2);
            graph.EdgeCount.Should().Be(1);

            var vertices = graph.Vertices.ToList();
            vertices.Should().Contain(v => v.Name == "n0");
            vertices.Should().Contain(v => v.Name == "n1");

            var edge = graph.Edges.FirstOrDefault();
            edge.Should().NotBeNull();
            edge.Source.Name.Should().Be("n0");
            edge.Target.Name.Should().Be("n1");
        }

        [Fact]
        public void ImportGraph_InvalidPath_ThrowsException()
        {
            // Arrange
            var invalidFilePath = Path.Combine(_tempDirectory, "nonexistent.graphml");

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", invalidFilePath);

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<CmdletInvocationException>();
        }


        [Fact]
        public void ImportGraph_InvalidGraphML_ThrowsException()
        {
            // Arrange
            var invalidGraphmlContent = "<invalid>this is not valid graphml</invalid>";

            var filePath = Path.Combine(_tempDirectory, "invalid.graphml");
            File.WriteAllText(filePath, invalidGraphmlContent);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", filePath);

            // Act
            //_powershell.Invoke();
            Action act = () => _powershell.Invoke();
            
            // Assert
            act.Should().Throw<CmdletInvocationException>().WithMessage("*graphml*");
        }

        [Fact]
        public void ImportGraph_EmptyFile_ThrowsException()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "empty.graphml");
            File.WriteAllText(filePath, string.Empty);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", filePath);

            // Act
            //_powershell.Invoke();
            Action act = () => _powershell.Invoke();
            
            // Assert
            act.Should().Throw<CmdletInvocationException>().WithMessage("*root element is missing*");
        }

        [Fact]
        public void ImportGraph_NoVerticesOrEdges()
        {
            // Arrange
            var graphmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<graphml xmlns='http://graphml.graphdrawing.org/xmlns'
    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
    xsi:schemaLocation='http://graphml.graphdrawing.org/xmlns
     http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd'>
  <graph id='G' edgedefault='directed'>
  </graph>
</graphml>";

            var filePath = Path.Combine(_tempDirectory, "emptygraph.graphml");
            File.WriteAllText(filePath, graphmlContent);

            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", filePath);

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            var graph = results[0].BaseObject as PsBidirectionalGraph;
            graph.Should().NotBeNull();
            graph.VertexCount.Should().Be(0);
            graph.EdgeCount.Should().Be(0);
        }

// TODO: Fix these tests if needed
//         [Fact]
//         public void ImportGraph_WithAttributes()
//         {
//             // Arrange
//             var graphmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
// <graphml xmlns='http://graphml.graphdrawing.org/xmlns'
//     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
//     xsi:schemaLocation='http://graphml.graphdrawing.org/xmlns
//      http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd'>
//   <key id='d0' for='node' attr.name='color' attr.type='string'/>
//   <key id='d1' for='edge' attr.name='weight' attr.type='double'/>
//   <graph id='G' edgedefault='directed'>
//     <node id='n0'>
//       <data key='d0'>red</data>
//     </node>
//     <node id='n1'>
//       <data key='d0'>blue</data>
//     </node>
//     <edge id='e0' source='n0' target='n1'>
//       <data key='d1'>1.5</data>
//     </edge>
//   </graph>
// </graphml>";

//             var filePath = Path.Combine(_tempDirectory, "attributes.graphml");
//             File.WriteAllText(filePath, graphmlContent);

//             _powershell.AddCommand("Import-Graph")
//                 .AddParameter("Path", filePath);

//             // Act
//             var results = _powershell.Invoke();

//             // Assert
//             results.Should().NotBeNullOrEmpty();
//             var graph = results[0].BaseObject as PsBidirectionalGraph;
//             graph.Should().NotBeNull();
//             graph.VertexCount.Should().Be(2);
//             graph.EdgeCount.Should().Be(1);

//             var vertices = graph.Vertices.ToList();
//             var vertex0 = vertices.FirstOrDefault(v => v.Name == "n0");
//             var vertex1 = vertices.FirstOrDefault(v => v.Name == "n1");

//             vertex0.Should().NotBeNull();
//             vertex1.Should().NotBeNull();

//             // Since PSVertex and PSEdgeTag might not handle attributes,
//             // you might need to extend them to store and access attributes if required.

//             var edges = graph.Edges.ToList();
//             var edge = edges.FirstOrDefault();

//             edge.Should().NotBeNull();
//             edge.Source.Name.Should().Be("n0");
//             edge.Target.Name.Should().Be("n1");

//             // Edge Tag verification (if implemented)
//             edge.Tag.Should().NotBeNull();
//         }

//         [Fact]
//         public void ImportGraph_WithInvalidXml_ThrowsException()
//         {
//             // Arrange
//             var invalidXmlContent = "<graphml><graph></graph></graphml"; // Missing closing '>' for graphml

//             var filePath = Path.Combine(_tempDirectory, "invalidxml.graphml");
//             File.WriteAllText(filePath, invalidXmlContent);

//             _powershell.AddCommand("Import-Graph")
//                 .AddParameter("Path", filePath);

//             // Act
//             _powershell.Invoke();

//             // Assert
//             _powershell.Streams.Error.Should().NotBeEmpty();
//             var error = _powershell.Streams.Error.First();
//             error.Exception.Should().BeOfType<XmlException>();
//         }

        [Fact]
        public void ImportGraph_NullPath_ThrowsException()
        {
            // Arrange
            _powershell.AddCommand("Import-Graph")
                .AddParameter("Path", null);

            // Act
            //_powershell.Invoke();

            Action act = () => _powershell.Invoke();

            act.Should().Throw<Exception>();

            // TODO: Fix this test
            // Assert
            // _powershell.Streams.Error.Should().NotBeEmpty();
            // var error = _powershell.Streams.Error.First();
            // error.Exception.Should().BeOfType<ParameterBindingValidationException>();
        }
    }
}
