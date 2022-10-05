using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;

namespace PSGraph.Tests;

[TestClass]
public class DsmViewTests
{
    [TestMethod]
    public void BasicDsmViewTest()
    {
        var dsm = new DsmClassic(TestData.DSMFull);
        var view = new DsmView(dsm);
        var s = view.ToSvg();
        var f = System.IO.Path.GetTempPath();
        var p = System.IO.Path.Combine(f, "dsmViewBasicTest.svg");
        s.Write(p);
    }
    
    [TestMethod]
    public void BasicDsmViewPartitionedTest()
    {
        var dsm = new DsmClassic(TestData.DSMFull);
        var algo = new DsmGraphPartitioningAlgorithm(dsm);
        var part = algo.Partition();
        var view = new DsmView(part, algo.Partitions);
        var s = view.ToSvg();
        var f = System.IO.Path.GetTempPath();
        var p = System.IO.Path.Combine(f, "dsmViewPartitionedGraphTest.svg");
        s.Write(p);
    }
    
    
    [TestMethod]
    public void BasicDsmGraphView()
    {
        var dsm = new DsmClassic(TestData.DSMFull);
        var algo = new DsmGraphPartitioningAlgorithm(dsm);
        var part = algo.Partition();
        var view = new DsmView(part, algo.Partitions);
        var s = view.ExportGraphViz();
        var f = System.IO.Path.GetTempPath();
        var p = System.IO.Path.Combine(f, "dsmGraphView.dot");
        File.WriteAllText(p, s);
    }
}