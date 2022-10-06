using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using PSGraph.Model;
using QuikGraph;

namespace PSGraph.Tests;

[TestClass]
public class ImportGraphCmdletTest
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
    public void TestImportGraphCmdlet()
    {
        try
        {
            var p = AppDomain.CurrentDomain.BaseDirectory;
            _powershell.AddCommand("Import-Graph");
            _powershell.AddParameters(new Dictionary<String, Object>
            {
                {"Path", System.IO.Path.Combine(p, "vms.graphml")}
            });

            Collection<PSObject>  result = _powershell.Invoke();
				
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (PsBidirectionalGraph));
        }
        finally
        {

            _powershell.Commands.Clear();
        }

    }
}