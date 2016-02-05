using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickGraph;

namespace PSGraph.Tests
{
	[TestClass]
	public class PsGraphUnitTest
	{
		static private PowerShell _powershell;

		[ClassInitialize()]
		public static void InitPsGraphUnitTest(TestContext tc)
		{
			_powershell = PowerShell.Create();
			_powershell.AddCommand("Import-Module")
				.AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof (PSGraph.PsGraph)));
			_powershell.Invoke();
			_powershell.Commands.Clear();
		}

		[ClassCleanup()]
		public static void CleanupPsGraphUnitTest()
		{
			_powershell?.Dispose();
		}

		[TestMethod]
		public void TestExistingGraphTypeNoProcessing_Success()
		{
			PSGraph.PsGraph psGraph = new PSGraph.PsGraph() {Type = PsGraphType.AdjacencyGraph};
			Assert.IsTrue(String.Equals(psGraph.Type, PsGraphType.AdjacencyGraph));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), "Unsupported graph.")]
		public void TestExistingGraphTypeNoProcessing_Unsuccess()
		{
			PSGraph.PsGraph psGraph = new PSGraph.PsGraph() { Type = (PsGraphType)0xC0FEE};
			Assert.IsTrue(String.Equals(psGraph.Type, (PsGraphType)0xC0FEE));
		}

		[TestMethod]
		public void TestAllGraphTypeProcessing_Success()
		{
			try
			{
				_powershell.AddCommand("New-Graph").AddParameter("Type", PsGraphType.AdjacencyGraph);
				Collection<PSObject> result = _powershell.Invoke();
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 1);
				Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (AdjacencyGraph<Object, Edge<Object>>));
				_powershell.Commands.Clear();

				_powershell.AddCommand("New-Graph").AddParameter("Type", PsGraphType.BidirectionalGraph);
				result = _powershell.Invoke();
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 1);
				Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (BidirectionalGraph<Object, Edge<Object>>));
				_powershell.Commands.Clear();

				_powershell.AddCommand("New-Graph").AddParameter("Type", PsGraphType.UndirectedGraph);
				result = _powershell.Invoke();
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 1);
				Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (UndirectedGraph<Object, Edge<Object>>));
				_powershell.Commands.Clear();
			}
			finally
			{

				_powershell.Commands.Clear();
			}

		}

		[TestMethod]
		[ExpectedException(typeof(ParameterBindingException), "Unsupported graph.")]
		public void TestAllGraphTypeProcessing_Unsuccess()
		{
			try
			{ 
				_powershell.AddCommand("New-Graph").AddParameter("Type", PsGraphType.BidirectionalMatrixGraph);
				Collection<PSObject> result = _powershell.Invoke();
				
			}
			finally
			{
				_powershell.Commands.Clear();
			}
		}
	}
}
