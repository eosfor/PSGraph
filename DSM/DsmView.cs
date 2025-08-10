using System.Drawing;
using MathNet.Numerics.Data.Text;
using PSGraph.Model;
using QuikGraph.Graphviz;
using Svg;
using Newtonsoft.Json.Linq;
using System.Text;

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
        return _dsm.DsmGraphView.ToGraphviz(a => a.FormatVertex += (sender, args) =>
        {
            args.VertexFormat.Label = args.Vertex.Label;
        });
    }

    public void ExportText(string Path)
    {
        DelimitedWriter.Write(Path, _dsm.DsmMatrixView, ",");
    }

    public string ExportText()
    {
        using var sw = new StringWriter();
        DelimitedWriter.Write(sw, _dsm.DsmMatrixView, ",");
        return sw.ToString();
    }

    private SvgDocument BuildSvgDocument()
    {
        var itemSize = 45;
        var h = _dsm.DsmMatrixView.ColumnCount * itemSize + itemSize;
        var w = _dsm.DsmMatrixView.RowCount * itemSize + itemSize;

        var svgDoc = new SvgDocument
        {
            Width = w,
            Height = h
        };

        GenerateMatrixViewAnnotations(itemSize, svgDoc);
        GenerateMatrixView(itemSize, svgDoc);
        if (_partitions != null)
            GeneratePartitionBoundaries(itemSize, svgDoc);

        return svgDoc;
    }

    public virtual SvgDocument ToSvg()
    {
        return BuildSvgDocument();
    }

    public virtual string ToSvgString()
    {
        var svgDoc = BuildSvgDocument();

        using var ms = new MemoryStream();
        svgDoc.Write(ms, useBom: false);
        ms.Position = 0;

        using var reader = new StreamReader(ms, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    // public string ToVegaSpec(VegaExportTypes exportType, string modulePath)
    // {
    //     var matrix = ToVegaReorderableMatrix(_dsm, _partitions);
    //     var vega = VegaHelper.GetVegaTemplateObjectFromModulePath(modulePath, "vega.dsm.matrix.json");

    //     // assuming these indices are correct for the matrix template
    //     vega.Data.Single(d => d.Name == "nodes").Values =
    //         matrix["nodes"]["values"].ToObject<List<object>>();

    //     vega.Data.Single(d => d.Name == "edges").Values =
    //         matrix["edges"]["values"].ToObject<List<object>>();

    //     return exportType switch
    //     {
    //         VegaExportTypes.HTML => VegaHelper.RenderHtml(vega),
    //         _ => vega.ToJson()
    //     };
    // }

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
                Stroke = new SvgColourServer(System.Drawing.Color.Black),
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
                    Stroke = new SvgColourServer(System.Drawing.Color.DimGray),
                    Fill = FillColor(i, j)
                };
                svgDoc.Children.Add(rect);
            }
        }

        SvgColourServer FillColor(int i, int j)
        {
            if (i == j) return new SvgColourServer(System.Drawing.Color.DarkGray);
            return _dsm.DsmMatrixView[j, i] == 1
                ? new SvgColourServer(System.Drawing.Color.SlateBlue)
                : new SvgColourServer(System.Drawing.Color.White);
        }
    }

    /// <summary>
    /// Формирует данные для reorderable-матрицы в формате Vega (v6).
    /// </summary>
    public JObject ToNodeAndEdgeView()
    {
        var nodeValues = new JArray();
        var edgeValues = new JArray();

        /* ------------------------ 1.  Сопоставляем вершину → группа ------------------------ */
        var vertexToGroup = new Dictionary<PSVertex, int>();

        // Группы нумеруем с 1; «непомеченные» вершины получат группу 0.
        if (_partitions is { Count: > 0 })
        {
            for (int gi = 0; gi < _partitions.Count; gi++)
                foreach (var v in _partitions[gi])
                    vertexToGroup[v] = gi + 1;
        }

        /* ------------------------ 2.  Узлы (nodes.values) ---------------------------------- */
        var rowKeys = _dsm.RowIndex
                         .OrderBy(kvp => kvp.Value)
                         .Select(kvp => kvp.Key)
                         .ToList();

        for (int i = 0; i < rowKeys.Count; i++)
        {
            var v = rowKeys[i];
            nodeValues.Add(new JObject
            {
                ["name"] = v.ToString(),
                ["index"] = i,
                ["group"] = vertexToGroup.TryGetValue(v, out var g) ? g : 0
            });
        }

        /* ------------------------ 3.  Рёбра (edges.values) --------------------------------- */
        var m = _dsm.DsmMatrixView;

        for (int r = 0; r < m.RowCount; r++)
            for (int c = 0; c < m.ColumnCount; c++)
            {
                if (m[r, c] == 0) continue;

                // определяем, внутри одной группы или межгрупповое ребро
                var srcGroup = (int)nodeValues[r]["group"];
                var dstGroup = (int)nodeValues[c]["group"];
                var edgeGroup = srcGroup == dstGroup ? srcGroup : -1;

                edgeValues.Add(new JObject
                {
                    ["source"] = r,
                    ["target"] = c,
                    ["group"] = edgeGroup,    // пригодится, если захотите подсвечивать связи
                                              // ["weight"] = m[r, c]   // при необходимости можно добавить вес
                    ["x"] = r,
                    ["y"] = c
                });
            }

        /* ------------------------ 4.  Итоговый объект для Vega ----------------------------- */
        return new JObject
        {
            ["nodes"] = new JObject { ["values"] = nodeValues },
            ["edges"] = new JObject { ["values"] = edgeValues }
        };
    }
}