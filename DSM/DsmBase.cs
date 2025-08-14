using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

public class DsmBase : IDsm
{
    private readonly Matrix<Double> _dsm;
    private readonly PsBidirectionalGraph _graph;
    private readonly Dictionary<PSVertex, int> _rowIndex;
    private readonly Dictionary<PSVertex, int> _colIndex;

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
        get
        {
            var ri = _rowIndex[from];
            var ti = _colIndex[to];
            return _dsm[ri, ti];
        } 
    }

    //public abstract IDsm Remove(PSVertex vertex);

    public IDsm Remove(PSVertex vertex)
    {
        if (!_rowIndex.ContainsKey(vertex))
            throw new KeyNotFoundException($"Vertex not found: {vertex.Label}");
        int rowIdx = _rowIndex[vertex];
        int colIdx = _colIndex[vertex];

        // Копируем граф и удаляем вершину
        var newGraph = new PsBidirectionalGraph(_graph);
        newGraph.RemoveVertex(vertex);

        // Копируем индексы и удаляем вершину
        var newRowIndex = new Dictionary<PSVertex, int>(_rowIndex);
        var newColIndex = new Dictionary<PSVertex, int>(_colIndex);
        newRowIndex.Remove(vertex);
        newColIndex.Remove(vertex);

        // Перестраиваем матрицу
        var newDsm = _dsm.RemoveRow(rowIdx).RemoveColumn(colIdx);

        // Пересчитываем индексы (сдвигаем)
        foreach (var v in newRowIndex.Keys.ToList())
        {
            if (newRowIndex[v] > rowIdx) newRowIndex[v]--;
        }
        foreach (var v in newColIndex.Keys.ToList())
        {
            if (newColIndex[v] > colIdx) newColIndex[v]--;
        }

        return new DsmBase(newDsm, newGraph, newRowIndex, newColIndex);
    }

    public IDsm Remove(List<PSVertex> vertices)
    {
        IDsm result = this;
        foreach (var v in vertices)
        {
            result = result.Remove(v);
        }
        return result;
    }

    public IDsm Order(List<PSVertex> order)
    {
        var dsmNew = Matrix<Double>.Build.Dense(order.Count, order.Count);

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


    public List<PSVertex> GetSinks()
    {
        return _graph.Vertices.Where(v => _graph.OutDegree(v) == 0).Distinct().ToList();
    }

    public List<PSVertex> GetSources()
    {
        return _graph.Vertices.Where(v => _graph.InDegree(v) == 0).Distinct().ToList();
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