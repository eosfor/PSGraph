using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Tests
{
    [TestClass]
    public class AddVertexCldletTests
    {
        static private PowerShell _powershell;

        [ClassInitialize()]
        public static void InitPsGraphUnitTest(TestContext tc)
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(PSGraph.Cmdlets.NewPsGraphCmdlet)));
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        [ClassCleanup()]
        public static void CleanupPsGraphUnitTest()
        {
            _powershell?.Dispose();
        }

        [TestMethod]
        public void AddVertex_Success()
        {
            var graph = new PsBidirectionalGraph();

            try
            {
                var vertex = "A";
                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }

            Assert.IsTrue(graph.VertexCount == 1);
        }

        [TestMethod]
        public void AddArbitratyObjectVertex_Success()
        {
            var graph = new PsBidirectionalGraph();

            try
            {
                var vertex = System.Diagnostics.Process.GetProcesses().First();
                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                Collection<PSObject> result = _powershell.Invoke();


            }
            finally
            {
                _powershell.Commands.Clear();
            }

            Assert.IsTrue(graph.VertexCount == 1);
        }

        [TestMethod]
        public void AddIdenticalArbitratyObjectVertex_Success()
        {
            var graph = new PsBidirectionalGraph();

            try
            {
                System.Diagnostics.Process vertex, vertex2;
                vertex = vertex2 = System.Diagnostics.Process.GetProcesses().First();
                
                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                Collection<PSObject> result = _powershell.Invoke();

                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }

            Assert.IsTrue(graph.VertexCount == 1);
        }

        [TestMethod]
        public void AddIdenticalVertex_Success()
        {
            var graph = new PsBidirectionalGraph();

            try
            {
                var vertex = "A";
                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                Collection<PSObject> result = _powershell.Invoke();

                _powershell.AddCommand("Add-Vertex");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Vertex", vertex}, { "Graph", graph }
                    });
                result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }

            Assert.IsTrue(graph.VertexCount == 1);
        }
    }
}
