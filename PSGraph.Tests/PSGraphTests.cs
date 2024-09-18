using Xunit;
using System;
using System.Management.Automation;
using PSGraph.Model;
using FluentAssertions;
using System.Collections.ObjectModel;

namespace PSGraph.Tests
{
    public class NewPsGraphCmdletTests : IDisposable
    {
        private PowerShell _powershell;

        public NewPsGraphCmdletTests()
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", typeof(PSGraph.Cmdlets.NewPsGraphCmdlet).Assembly);
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        public void Dispose()
        {
            _powershell.Dispose();
        }

        [Fact]
        public void NewGraph_CreatesEmptyGraph()
        {
            // Arrange
            _powershell.AddCommand("New-Graph");

            // Act
            var results = _powershell.Invoke();

            // Assert
            results.Should().NotBeNullOrEmpty();
            results.Count.Should().Be(1);
            var graph = results[0].BaseObject as PsBidirectionalGraph;
            graph.Should().NotBeNull();
            graph.VertexCount.Should().Be(0);
            graph.EdgeCount.Should().Be(0);
        }

        [Fact]
        public void NewGraph_WithInvalidParameter_ThrowsException()
        {
            // Arrange
            _powershell.AddCommand("New-Graph")
                .AddParameter("InvalidParameter", "value");

            // Act
            Action act = () => _powershell.Invoke();

            // Assert
            act.Should().Throw<ParameterBindingException>()
                .WithMessage("*A parameter cannot be found that matches parameter name*");
        }

    }
}
