using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public class DsmClassic: DsmBase
{
    
    public DsmClassic(PsBidirectionalGraph g) : base(g)
    {
    }

    public DsmClassic(DsmClassic current) : base(current)
    {
    }

    protected DsmClassic(Matrix<float> dsm, PsBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex) : base(dsm, graph, rowIndex, colIndex)
    {
    }
    
}