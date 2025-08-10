using Newtonsoft.Json.Linq;
using PSGraph.Model;
using Svg;

namespace PSGraph.DesignStructureMatrix
{
    public interface IDsmView
    {
        public abstract SvgDocument ToSvg();
        public abstract string ToSvgString();
        public abstract void ExportText(string Path);
        public abstract string ExportText();
        //public abstract string ToVegaSpec(VegaExportTypes exportType, string modulePath);
        public string ExportGraphViz();
        public JObject ToNodeAndEdgeView();
    }
}