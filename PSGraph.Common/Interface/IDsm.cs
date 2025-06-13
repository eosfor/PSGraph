using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public interface IDsm
{
    public Matrix<Single> DsmMatrixView { get; }
    public PsBidirectionalGraph DsmGraphView { get; }
    
    public Matrix<Single> DsmMatrixViewCopy { get; }
    public PsBidirectionalGraph DsmGraphViewCopy { get; }

    public Dictionary<PSVertex, int> RowIndex { get; }
    public Dictionary<PSVertex, int> ColIndex { get; }   
    public Single this[PSVertex from, PSVertex to] { get; }

    public IDsm Remove(PSVertex vertex);
    public IDsm Remove(List<PSVertex> vertex);

    public IDsm Order(List<PSVertex> partitions);
}