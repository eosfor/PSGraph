using System.Management.Automation;
using PSGraph.Helpers;
using PSGraph.Model;
using System.Xml;
using QuikGraph.Serialization;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Import, "Graph")]
    public class ImportGraphCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Path = null!;

        [Parameter(Mandatory = false)]
        public GraphImportTypes Format = GraphImportTypes.GraphML;

        [Parameter(Mandatory = false)]
        public string FromColumn = "From";

        [Parameter(Mandatory = false)]
        public string ToColumn = "To";

        [Parameter(Mandatory = false)]
        public char Delimiter = ',';

        [Parameter(Mandatory = false)]
        public SwitchParameter NoHeader;

        protected override void ProcessRecord()
        {
            var resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);

            if (!File.Exists(resolvedPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"File not found: {resolvedPath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            PsBidirectionalGraph graph;

            switch (Format)
            {
                case GraphImportTypes.GraphML:
                    graph = ImportGraphML(resolvedPath);
                    break;
                case GraphImportTypes.Csv:
                    graph = CsvGraphImporter.Import(resolvedPath, FromColumn, ToColumn, Delimiter, NoHeader.IsPresent);
                    break;
                case GraphImportTypes.Json:
                    graph = JsonGraphImporter.Import(resolvedPath);
                    break;
                default:
                    ThrowTerminatingError(new ErrorRecord(
                        new NotSupportedException($"Unsupported import format: {Format}"),
                        "UnsupportedFormat",
                        ErrorCategory.InvalidArgument,
                        Format));
                    return;
            }

            WriteObject(graph);
        }

        private static PsBidirectionalGraph ImportGraphML(string path)
        {
            var graph = new PsBidirectionalGraph(false);
            using var xmlReader = XmlReader.Create(path);
            graph.DeserializeFromGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(
                xmlReader,
                id => new PSVertex(id),
                (source, target, id) => new PSEdge(source, target, new PSEdgeTag()));
            return graph;
        }
    }
}
