using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

namespace PSGraph.Tests;

[TestClass]
public class StartDsmClusteringCmdletTest
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
    public void RunClassicClusteringTest_NoAlgoParam()
    {
        var dsm = new DsmClassic(TestData.DSMFull);
        try
        {
            _powershell.AddCommand("Start-DsmClustering");
            _powershell.AddParameters(new Dictionary<String, Object>
            {
                {"Dsm", dsm}
            });

            Collection<PSObject>  result = _powershell.Invoke();
				
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (PartitioningResult));
        }
        finally
        {

            _powershell.Commands.Clear();
        }
    }
    
    [TestMethod]
    public void RunClassicClusteringTest_GraphAlgoParam()
    {
        var dsm = new DsmClassic(TestData.DSMFull);
        try
        {
            _powershell.AddCommand("Start-DsmClustering");
            _powershell.AddParameters(new Dictionary<String, Object>
            {
                {"Dsm", dsm}, {"ClusteringAlgorithm", DsmPartitioningAlgorithms.GraphBased}
            });

            Collection<PSObject>  result = _powershell.Invoke();
				
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ImmediateBaseObject.GetType() == typeof (PartitioningResult));
        }
        finally
        {

            _powershell.Commands.Clear();
        }
    }

    [TestMethod]
    public void RunGraphBasedClusteringTest()
    {
        
    }
}