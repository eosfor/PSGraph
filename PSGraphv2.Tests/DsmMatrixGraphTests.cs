using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;
using QuikGraph;
using QuikGraph.Graphviz.Dot;
using QuikGraph.Graphviz;
using System;
using System.Collections.Generic;
using PSGraph.Model;

namespace PSGraph.Tests;

[TestClass]
public class DsmMatrixGraphTests
{
    [TestMethod]
    public void DsmMatrixGraphPartitionTest()
    {
        var dsm = new DsmMatrixGraph(TestData.DSMFull);
        var partitioned = dsm.Partition();
    }
}