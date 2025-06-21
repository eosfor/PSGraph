using Svg;

namespace PSGraph.DesignStructureMatrix
{
    public interface IDsmView
    {
        public abstract SvgDocument ToSvg();
        public abstract void ExportText(string Path);
        public string ExportGraphViz();
    }
}