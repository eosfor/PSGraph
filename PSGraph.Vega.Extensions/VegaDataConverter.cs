using PSGraph.Model;
using PSGraph.Model.VegaDataModels;
using QuikGraph;

namespace PSGraph.Vega.Extensions
{
    public static class VegaExtensions
    {
        public static List<GraphRecord> ConvertToParentChildList(this Model.PsBidirectionalGraph graph)
        {
            var records = new List<GraphRecord>();
            var vertexLookup = new Dictionary<PSVertex, int>();

            int idCounter = 0;

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
                        record.parent = sourceId;
                    }
                }
            }

            return records;
        }
    }
}
