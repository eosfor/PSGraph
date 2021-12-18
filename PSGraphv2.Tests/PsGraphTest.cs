using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.Model;
using QuikGraph;

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
				.AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof (PSGraph.NewPsGraphCmdlet)));
			_powershell.Invoke();
			_powershell.Commands.Clear();
		}

		[ClassCleanup()]
		public static void CleanupPsGraphUnitTest()
		{
			_powershell?.Dispose();
		}

		[TestMethod]
		public void TestAllGraphTypeProcessing_Success()
		{
			try
			{
				_powershell.AddCommand("New-Graph");
				Collection<PSObject>  result = _powershell.Invoke();
				Assert.IsTrue(result != null);
				Assert.IsTrue(result.Count == 1);
				Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (PSBidirectionalGraph));
				_powershell.Commands.Clear();
			}
			finally
			{

				_powershell.Commands.Clear();
			}

		}
	}
}
