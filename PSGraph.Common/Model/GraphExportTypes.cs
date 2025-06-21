namespace PSGraph
{
    public enum GraphExportTypes
    {
        Graphviz,
        GraphML,
        MSAGL_MDS,
        MSAGL_SUGIYAMA,
        MSAGL_FASTINCREMENTAL,
        Vega_ForceDirected,
        Vega_AdjacencyMatrix,
        Vega_TreeLayout
    }

    public enum VegaExportTypes
    {
        HTML,
        JSON,
        SVG,
        DOT,
        GRAPHML
    }
}