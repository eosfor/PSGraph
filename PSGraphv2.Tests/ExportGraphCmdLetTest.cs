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

// -graph $g -format graphviz -path c:\temp\graph.txt
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
                .AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(PSGraph.NewPsGraphCmdlet)));
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
            var graph = new PSBidirectionalGraph();
            PSGraph.ExportGraphCmdLet export = new PSGraph.ExportGraphCmdLet();
            PSGraph.GraphExportTypes expType = export.Format;

            export.Graph = graph;

            Assert.IsTrue(Object.Equals(export.Graph, graph));
            Assert.IsTrue(export.Path == null);
            Assert.IsTrue(export.Format == expType);
        }

        [TestMethod]
        public void TestPathPropertyNoProcessing_Success()
        {
            PSGraph.ExportGraphCmdLet export = new PSGraph.ExportGraphCmdLet();
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
            ExportGraphCmdLet export = new ExportGraphCmdLet() { Format = GraphExportTypes.Graphviz};
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
        public void TestExportGraphProcessing_Success()
        {
            try
            {
                //var graph = new PSBidirectionalGraph()();
                var graph = new PSBidirectionalGraph();
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


    }
}
