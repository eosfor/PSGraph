using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix
{
    public interface IDsmView
    {
        public abstract void ExportText(string Path);
        public abstract string ExportText();
        //public abstract string ToVegaSpec(VegaExportTypes exportType, string modulePath);
        public string ExportGraphViz();
    }
}