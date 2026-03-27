using MathNet.Numerics.Data.Text;
using PSGraph.Model;
using QuikGraph.Graphviz;

namespace PSGraph.DesignStructureMatrix;

public class DsmView : IDsmView
{
    private readonly IDsm _dsm;

    public DsmView(IDsm dsm, List<List<PSVertex>>? partitions = null)
    {
        _dsm = dsm;
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

}