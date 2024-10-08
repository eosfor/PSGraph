using System.Drawing;
using MathNet.Numerics.Data.Text;
using PSGraph.Model;
using QuikGraph.Graphviz;
using Svg;

namespace PSGraph.DesignStructureMatrix;

public class DsmView : IDsmView
{
    private readonly List<List<PSVertex>>? _partitions;
    private readonly IDsm _dsm;

    public DsmView(IDsm dsm, List<List<PSVertex>>? partitions = null)
    {
        _dsm = dsm;
        _partitions = partitions;
    }

    public string ExportGraphViz()
    {
        return _dsm.DsmGraphView.ToGraphviz( a => a.FormatVertex += (sender, args) =>
        {
            args.VertexFormat.Label = args.Vertex.Label; });
    }

    public void ExportText(string Path)
    {
        DelimitedWriter.Write(Path, _dsm.DsmMatrixView, ",");
    }

    public virtual SvgDocument ToSvg()
    {
        var itemSize = 45; // the size of a cell of a matrix
        var h = _dsm.DsmMatrixView.ColumnCount * itemSize + itemSize;
        var w = _dsm.DsmMatrixView.RowCount * itemSize + itemSize;

        var svgDoc = new SvgDocument
        {
            Width = w,
            Height = h
        };

        GenerateMatrixViewAnnotations(itemSize, svgDoc);
        GenerateMatrixView(itemSize, svgDoc);
        if (null != _partitions) GeneratePartitionBoundaries(itemSize, svgDoc);
        return svgDoc;
    }

    private void GeneratePartitionBoundaries(int itemSize, SvgDocument svgDoc)
    {
        foreach (var partition in _partitions)
        {
            var w = partition.Count * itemSize;
            var h = partition.Count * itemSize;
            var row = _dsm.RowIndex[partition.First()] * itemSize + itemSize;

            var rect = new SvgRectangle
            {
                Width = w,
                Height = h,
                X = row,
                Y = row, //top left corner
                StrokeWidth = (float)2.0,
                Stroke = new SvgColourServer(Color.Black),
                FillOpacity = 0
            };

            svgDoc.Children.Add(rect);
        }
    }

    private void GenerateMatrixViewAnnotations(int itemSize, SvgDocument svgDoc)
    {
        var y = 0;
        var x = 0;

        foreach (var item in _dsm.RowIndex)
        {
            var el = new SvgText(item.Key.ToString());
            x = itemSize - 15; // magic number as FontSize is not a square
            el.X.Add(x);
            el.Y.Add((y + 1) * itemSize + itemSize / 2);
            svgDoc.Children.Add(el);
            y++;
        }

        x = 0;
        y = 0;
        foreach (var item in _dsm.ColIndex)
        {
            var el = new SvgText(item.Key.ToString());
            y = itemSize - (int)el.FontSize - 5;
            el.X.Add((x + 1) * itemSize + itemSize / 2);
            el.Y.Add(y);
            svgDoc.Children.Add(el);
            x++;
        }
    }

    private void GenerateMatrixView(int itemSize, SvgDocument svgDoc)
    {
        for (var i = 0; i < _dsm.DsmMatrixView.ColumnCount; i++)
        {
            var x = i * itemSize + itemSize;
            for (var j = 0; j < _dsm.DsmMatrixView.RowCount; j++)
            {
                var y = j * itemSize + itemSize;
                var rect = new SvgRectangle
                {
                    Width = itemSize,
                    Height = itemSize,
                    X = x,
                    Y = y,
                    StrokeWidth = (float)0.5,
                    Stroke = new SvgColourServer(Color.DimGray),
                    Fill = FillColor(i, j)
                };
                svgDoc.Children.Add(rect);
            }
        }

        SvgColourServer FillColor(int i, int j)
        {
            if (i == j) return new SvgColourServer(Color.DarkGray);
            return _dsm.DsmMatrixView[j, i] == 1
                ? new SvgColourServer(Color.SlateBlue)
                : new SvgColourServer(Color.White);
        }
    }
}