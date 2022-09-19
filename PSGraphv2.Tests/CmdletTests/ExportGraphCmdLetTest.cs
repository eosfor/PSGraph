using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuikGraph;
using System.Collections.ObjectModel;
using PSGraph.Model;

namespace PSGraph.Tests
{
    [TestClass]
    public class ExportGraphCmdLetTest
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
        public void TestGraphPropertyNoProcessing_Success()
        {
            var graph = new PsBidirectionalGraph();
            PSGraph.Cmdlets.ExportGraphViewCmdLet export = new PSGraph.Cmdlets.ExportGraphViewCmdLet();
            PSGraph.GraphExportTypes expType = export.Format;

            export.Graph = graph;

            Assert.IsTrue(Object.Equals(export.Graph, graph));
            Assert.IsTrue(export.Path == null);
            Assert.IsTrue(export.Format == expType);
        }

        [TestMethod]
        public void TestPathPropertyNoProcessing_Success()
        {
            PSGraph.Cmdlets.ExportGraphViewCmdLet export = new PSGraph.Cmdlets.ExportGraphViewCmdLet();
            PSGraph.GraphExportTypes expType = export.Format;

            String originPath = @"C:\Temp";
            export.Path = originPath;

            Assert.IsTrue(export.Graph == null);
            Assert.IsTrue(String.Equals(export.Path, originPath));
            Assert.IsTrue(export.Format == expType);
        }

        [TestMethod]
        public void TestExistingExportTypeNoProcessing_Success()
        {
            PSGraph.Cmdlets.ExportGraphViewCmdLet export = new PSGraph.Cmdlets.ExportGraphViewCmdLet() { Format = GraphExportTypes.Graphviz};
            Assert.IsTrue(Enum.Equals(export.Format, GraphExportTypes.Graphviz));
        }


        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException), "Graph null", AllowDerivedTypes = true)]
        public void TestNullGraphProcessing_Unsuccess()
        {
            try
            {
                Object graph = null;
               _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", GraphExportTypes.Graphviz}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }


        [TestMethod]
        public void TestGraphvizExport_Success()
        {
            var graph = TestData.SimpleTestGraph1;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "graphvis-export-test.gv");

            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", GraphExportTypes.Graphviz}, { "Path", p }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void TestGraphMLExport_Success()
        {
            var graph = TestData.SimpleTestGraph1;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "graphml-export-test.graphml");
            
            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", GraphExportTypes.GraphML}, { "Path", p }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void Export_MSAGL_MDS_Powershell_Success()
        {
            var graph = TestData.SimpleTestGraph2;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "export-test-MDS.svg");

            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Format", GraphExportTypes.MSAGL_MDS}, { "Graph", graph }, { "Path", p }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void Export_MSAGL_SUGIYAMA_Powershell_Success()
        {
            var graph = TestData.SimpleTestGraph2;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "export-test-SUGYYAMA.svg");

            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Format", GraphExportTypes.MSAGL_SUGIYAMA}, { "Graph", graph }, { "Path", p }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void Export_MSAGL_FASTINCREMENT_Powershell_Success()
        {
            var graph = TestData.SimpleTestGraph2;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "export-test-FASTINCREMENT.svg");

            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Format", GraphExportTypes.MSAGL_FASTINCREMENTAL}, { "Graph", graph }, { "Path", p }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }
    }
}
