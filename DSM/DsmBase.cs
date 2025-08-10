using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public class DsmBase : IDsm
{
    protected readonly Matrix<Double> _dsm;
    protected readonly PsBidirectionalGraph _graph;
    protected Dictionary<PSVertex, int> _rowIndex;
    protected Dictionary<PSVertex, int> _colIndex;

    public Dictionary<PSVertex, int> RowIndex => _rowIndex;
    public Dictionary<PSVertex, int> ColIndex => _colIndex;
    public Matrix<Double> DsmMatrixView => _dsm;
    public PsBidirectionalGraph DsmGraphView => _graph;

    private const int _linkWeight = 1;

    public IDsm Clone()
    {
        return new DsmBase(this);
    }

    public Matrix<Double> DsmMatrixViewCopy
    {
        get
        {
            return CopyDsmMatrix(_dsm);

            Matrix<Double> CopyDsmMatrix(Matrix<Double> dsm)
            {
                var ret = Matrix<Double>.Build.Dense(dsm.RowCount, dsm.ColumnCount);
                dsm.CopyTo(ret);
                return ret;
            }
        }
    }

    public PsBidirectionalGraph DsmGraphViewCopy
    {
        get
        {
            return CopyGraph(_graph);

            PsBidirectionalGraph CopyGraph(PsBidirectionalGraph graph)
            {
                return new PsBidirectionalGraph(graph);
            }
        }
    }

    public Double this[PSVertex from, PSVertex to]
    {
        get => _dsm[_rowIndex[from], _colIndex[to]];
    }

    //public abstract IDsm Remove(PSVertex vertex);

    public virtual IDsm Remove(PSVertex vertex)
    {
        //remove row and column from the matrix
        //remove vertex from the graph
        //update index
        var matrixCopy = _dsm.RemoveRow(_rowIndex[vertex]);
        matrixCopy = matrixCopy.RemoveColumn(_colIndex[vertex]);

        var graphCopy = new PsBidirectionalGraph(_graph);

        graphCopy.RemoveVertex(vertex);

        var newRowIndex = new Dictionary<PSVertex, int>(_rowIndex);
        var newColIndex = new Dictionary<PSVertex, int>(_colIndex);

        newRowIndex.Remove(vertex);
        newColIndex.Remove(vertex);

        foreach (var r in newRowIndex.Keys)
        {
            if (newRowIndex[r] > _rowIndex[vertex]) newRowIndex[r] -= 1;
        }

        foreach (var r in newColIndex.Keys)
        {
            if (newColIndex[r] > _colIndex[vertex]) newColIndex[r] -= 1;
        }


        return new DsmBase(matrixCopy, graphCopy, newRowIndex, newColIndex);
    }

    public IDsm Remove(List<PSVertex> vertex)
    {
        IDsm ret = this;
        foreach (var v in vertex)
        {
            ret = ret.Remove(v);
        }

        return ret;
    }

    public IDsm Order(List<PSVertex> order)
    {
        var dsmNew = Matrix<Double>.Build.Dense(_dsm.RowCount, _dsm.ColumnCount);

        int k = 0;
        var newRowIndex = order.ToDictionary(v => v, v => k++);

        k = 0;
        var newColIndex = order.ToDictionary(v => v, v => k++);

        foreach (var i in newRowIndex)
        {
            foreach (var j in newColIndex)
            {
                dsmNew[i.Value, j.Value] = this[i.Key, j.Key];
            }
        }


        return new DsmBase(dsmNew, _graph, newRowIndex, newColIndex);
    }

    private Matrix<Double> InitDsmWithGraph(PsBidirectionalGraph graph)
    {
        var dsm = Matrix<Double>.Build.Dense(graph.VertexCount, graph.VertexCount);

        foreach (var e in graph.Edges)
        {
            var from = e.Source;
            var to = e.Target;

            var colIndex = _colIndex[to];
            var rowIndex = _rowIndex[from];
            dsm[rowIndex, colIndex] = _linkWeight;
        }

        return dsm;
    }

    private Dictionary<PSVertex, int> InitIndexWithGraph(PsBidirectionalGraph graph)
    {
        int i = 0;
        var ret = graph.Vertices.ToDictionary(v => v, v => i++);

        return ret;
    }

    protected DsmBase(PsBidirectionalGraph g)
    {
        _graph = g;
        _rowIndex = InitIndexWithGraph(g);
        _colIndex = InitIndexWithGraph(g);
        _dsm = InitDsmWithGraph(g);
    }

    protected DsmBase(IDsm current)
    {
        _rowIndex = new Dictionary<PSVertex, int>(current.RowIndex);
        _colIndex = new Dictionary<PSVertex, int>(current.ColIndex);
        _graph = new PsBidirectionalGraph(current.DsmGraphView);
        _dsm = Matrix<Double>.Build.Dense(current.DsmMatrixView.RowCount, current.DsmMatrixView.ColumnCount);
        current.DsmMatrixView.CopyTo(_dsm);
    }

    protected DsmBase(Matrix<Double> dsm, PsBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
    {
        _dsm = dsm;
        _graph = graph;
        _rowIndex = rowIndex;
        _colIndex = colIndex;
    }
}