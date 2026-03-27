using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public interface IDsm
{
    public Matrix<double> DsmMatrixView { get; }
    public PsBidirectionalGraph DsmGraphView { get; }

    public Matrix<double> DsmMatrixViewCopy { get; }
    public PsBidirectionalGraph DsmGraphViewCopy { get; }

    public Dictionary<PSVertex, int> RowIndex { get; }
    public Dictionary<PSVertex, int> ColIndex { get; }
    public double this[PSVertex from, PSVertex to] { get; }

    public List<PSVertex> GetSinks();
    public List<PSVertex> GetSources();

    public IDsm Remove(PSVertex vertex);
    public IDsm Remove(List<PSVertex> vertex);
    public IDsm Order(List<PSVertex> partitions);
    public IDsm Clone();
}