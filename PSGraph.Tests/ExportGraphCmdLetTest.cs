using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickGraph;
using System.Collections.ObjectModel;

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
            var graph = new AdjacencyGraph<Object, STaggedEdge<Object, Object>>();
            PSGraph.ExportGraphCmdLet export = new PSGraph.ExportGraphCmdLet();
            PSGraph.ExportTypes expType = export.Format;

            export.Graph = graph;

            Assert.IsTrue(Object.Equals(export.Graph, graph));
            Assert.IsTrue(export.Path == null);
            Assert.IsTrue(export.Format == expType);
        }

        [TestMethod]
        public void TestPathPropertyNoProcessing_Success()
        {
            PSGraph.ExportGraphCmdLet export = new PSGraph.ExportGraphCmdLet();
            PSGraph.ExportTypes expType = export.Format;

            String originPath = @"C:\Temp";
            export.Path = originPath;

            Assert.IsTrue(export.Graph == null);
            Assert.IsTrue(String.Equals(export.Path, originPath));
            Assert.IsTrue(export.Format == expType);
        }

        [TestMethod]
        public void TestExistingExportTypeNoProcessing_Success()
        {
            ExportGraphCmdLet export = new ExportGraphCmdLet() { Format = ExportTypes.Graphviz};
            Assert.IsTrue(Enum.Equals(export.Format, ExportTypes.Graphviz));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Unsupported export type.")]
        public void TestExistingExportTypeNoProcessing_Unsuccess()
        {
            ExportGraphCmdLet export = new ExportGraphCmdLet() { Format = (ExportTypes)0xC0FEE };
            Assert.IsTrue(Enum.Equals(export.Format, (ExportTypes)0xC0FEE));
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
                        {"graph", graph}, {"format", ExportTypes.Graphviz}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CmdletInvocationException), "Unsupported graph type.")]
        public void TestUnsupportedGraphTypeProcessing_Unsuccess()
        {
            try
            {
                Object graph = new Object();
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", ExportTypes.Graphviz}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException), "Unsupported export type.")]
        public void TestUnsupportedExportTypeProcessing_Unsuccess()
        {
            try
            {
                var graph = new AdjacencyGraph<Object, STaggedEdge<Object, Object>>();
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", (ExportTypes)0xC0FEE}
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
                var graph = new AdjacencyGraph<Object, STaggedEdge<Object, Object>>();
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"graph", graph}, {"format", ExportTypes.Graphviz}
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
