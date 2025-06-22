using Microsoft.Msagl.Core.Layout;
using PSGraph.Model;
using PSGraph.Model.VegaDataModels;
using QuikGraph;

namespace PSGraph.Vega.Extensions
{
    public static class VegaConverterExtensions
    {
        public static (List<NodeRecord> nodes, List<LinkRecord> links) ConvertToVegaNodeLink(this Model.PsBidirectionalGraph graph)
        {
            var nodes = new List<NodeRecord>();
            var links = new List<LinkRecord>();
            var vertexLookup = new Dictionary<PSVertex, int>();

            int idCounter = 0;

            foreach (var vertex in graph.Vertices)
            {
                if (!vertexLookup.ContainsKey(vertex))
                {
                    vertexLookup[vertex] = idCounter;
                }
                var group = 1;
                if (((IDictionary<string, object?>)vertex.Metadata).ContainsKey("group"))
                {
                    var groupObj = ((IDictionary<string, object?>)vertex.Metadata)["group"];
                    if (groupObj != null && int.TryParse(groupObj.ToString(), out int parsedGroup))
                        group = parsedGroup;
                }
                nodes.Add(new NodeRecord(vertex.Label, group, idCounter++));
            }

            foreach (var edge in graph.Edges)
            {
                if (vertexLookup.TryGetValue(edge.Source, out int sourceId) &&
                    vertexLookup.TryGetValue(edge.Target, out int targetId))
                {
                    links.Add(new LinkRecord(sourceId, targetId, 1, sourceId, targetId));
                }
            }

            return (nodes, links);
        }
        public static List<IGraphRecord> ConvertToParentChildList(this Model.PsBidirectionalGraph graph)
        {
            var records = new List<IGraphRecord>();
            var vertexLookup = new Dictionary<PSVertex, int>();

            int idCounter = 1;

            foreach (var vertex in graph.Vertices)
            {
                if (!vertexLookup.ContainsKey(vertex))
                {
                    vertexLookup[vertex] = idCounter++;
                }
                records.Add(new GraphRecord
                {
                    id = vertexLookup[vertex],
                    name = vertex.Label,
                    parent = -1 // Default parent value, can be updated later
                });
            }

            foreach (var edge in graph.Edges)
            {
                if (vertexLookup.TryGetValue(edge.Source, out int sourceId) &&
                    vertexLookup.TryGetValue(edge.Target, out int targetId))
                {
                    var record = records.Find(r => r.id == targetId);
                    if (record != null)
                    {
                        ((GraphRecord)record).parent = sourceId;
                    }
                }
            }

            var rootRecords = records
                .Where(r => ((GraphRecord)r).parent == -1)
                .ToList();

            // Удаляем их из исходного списка
            foreach (var root in rootRecords)
            {
                records.Remove(root);
            }

            // Преобразуем их в GraphRootRecord
            var roots = rootRecords
                .Select(r => new GraphRootRecord
                {
                    id = r.id,
                    name = r.name
                })
                .ToList();

            records.AddRange(roots);
            return records;
        }
    }
}
