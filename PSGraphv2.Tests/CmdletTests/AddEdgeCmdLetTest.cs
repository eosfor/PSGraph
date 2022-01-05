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

//add-edge -from $nodeFrom -to $nodeTo -attributes $attr -graph $g
namespace PSGraph.Tests
{
    [TestClass]
    public class AddEdgeCmdLetTest
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
            var graph = new PSBidirectionalGraph();
            PSGraph.Cmdlets.AddEdgeCmdLet psGraph = new PSGraph.Cmdlets.AddEdgeCmdLet() { Graph = graph };

            Assert.IsTrue(Object.Equals(psGraph.Graph, graph));
            Assert.IsTrue(psGraph.From == null);
            Assert.IsTrue(psGraph.To == null);
            Assert.IsTrue(psGraph.Tag == null);
        }

        [TestMethod]
        public void TestFromPropertyNoProcessing_Success()
        {
            var graph = new PSBidirectionalGraph();
            PSGraph.Cmdlets.AddEdgeCmdLet psGraph = new PSGraph.Cmdlets.AddEdgeCmdLet() { From = graph };

            Assert.IsTrue(Object.Equals(psGraph.From, graph));
            Assert.IsTrue(psGraph.Graph == null);
            Assert.IsTrue(psGraph.To == null);
            Assert.IsTrue(psGraph.Tag == null);
        }

        [TestMethod]
        public void TestToPropertyNoProcessing_Success()
        {
            var graph = new PSBidirectionalGraph();
            PSGraph.Cmdlets.AddEdgeCmdLet psGraph = new PSGraph.Cmdlets.AddEdgeCmdLet() { To = graph };

            Assert.IsTrue(Object.Equals(psGraph.To, graph));
            Assert.IsTrue(psGraph.Graph == null);
            Assert.IsTrue(psGraph.From == null);
            Assert.IsTrue(psGraph.Tag == null);
        }

        [TestMethod]
        public void TestAttributePropertyNoProcessing_Success()
        {
            var obj = new Object();
            PSGraph.Cmdlets.AddEdgeCmdLet psGraph = new PSGraph.Cmdlets.AddEdgeCmdLet() { Tag = obj };

            Assert.IsTrue(Object.Equals(psGraph.Tag, obj));
            Assert.IsTrue(psGraph.Graph == null);
            Assert.IsTrue(psGraph.From == null);
            Assert.IsTrue(psGraph.To == null);
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException), "Graph object is missing.", AllowDerivedTypes=true)]
        public void TestMissingGraphParameterProcessing_Unsuccess()
        {
            try
            {
                Object to = new Object();
                Object from = new Object();
                Object attr = new Object();
                _powershell.AddCommand("Add-Edge");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"From", from}, {"To", to}, {"Attribute", attr}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException), "To object is missing.", AllowDerivedTypes = true)]
        public void TestMissingToParameterProcessing_Unsuccess()
        {
            try
            {
                var graph = new PSBidirectionalGraph();
                Object to = null;
                Object from = new Object();
                Object attr = new Object();
                _powershell.AddCommand("Add-Edge");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"From", from}, {"To", to}, {"Attribute", attr}, {"Graph", graph}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException), "To object is missing.", AllowDerivedTypes = true)]
        public void TestMissingFromParameterProcessing_Unsuccess()
        {
            try
            {
                var graph = new PSBidirectionalGraph();
                Object to = new Object();
                Object from = null;
                Object attr = new Object();
                _powershell.AddCommand("Add-Edge");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"From", from}, {"To", to}, {"Attribute", attr}, {"Graph", graph}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void TestAddEdgeWithAttrProcessing_Success()
        {
            try
            {
                var graph = new PSBidirectionalGraph();
                Object to = new Object();
                Object from = new Object();
                Object tag = new Object();
                _powershell.AddCommand("Add-Edge");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"From", from}, {"To", to}, {"Tag", tag}, {"Graph", graph}
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void TestAddEdgeWithoutAttrProcessing_Success()
        {
            try
            {
                var graph = new PSBidirectionalGraph();
                string to = "to";
                string from = "from";
                _powershell.AddCommand("Add-Edge");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"From", from}, {"To", to}, {"Graph", graph}
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
