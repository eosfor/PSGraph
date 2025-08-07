using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using PSGraph.Model.VegaDataModels;
using QuikGraph;

namespace PSGraph.Vega.Extensions
{
    public class MatrixCellRecord
    {
        public string row { get; set; } = null!;
        public string col { get; set; } = null!;
        public float value { get; set; }
    }
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
                string nodeType = vertex.OriginalObject != null ? vertex.OriginalObject.GetType().ToString() : "";
                nodes.Add(new NodeRecord(vertex.Label, group, nodeType, idCounter++));
            }

            foreach (var edge in graph.Edges)
            {
                int value = 1;
                if (vertexLookup.TryGetValue(edge.Source, out int sourceId) &&
                    vertexLookup.TryGetValue(edge.Target, out int targetId))
                {
                    {
                        if (((IDictionary<string, object?>)edge.Target.Metadata).ContainsKey("group"))
                        {
                            var groupObj = ((IDictionary<string, object?>)edge.Target.Metadata)["group"];
                            if (groupObj != null && int.TryParse(groupObj.ToString(), out int parsedGroup))
                                value = parsedGroup;
                        }

                    }
                    links.Add(new LinkRecord(sourceId, targetId, value, sourceId, targetId));
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


        public static (JArray nodes, JArray edges) ToVegaReorderableMatrix(this IDsm dsm, List<List<PSVertex>> partitions)
        {
            var nodes = new JArray();
            var edges = new JArray();

            var vertexToGroup = new Dictionary<PSVertex, int>();
            if (partitions is { Count: > 0 })
            {
                for (int gi = 0; gi < partitions.Count; gi++)
                {
                    foreach (var v in partitions[gi])
                        vertexToGroup[v] = gi + 1;   // группы нумеруем с 1
                }
            }

            var rowKeys = dsm.RowIndex.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            //var indexMap = rowKeys.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);

            for (int i = 0; i < rowKeys.Count; i++)
            {
                var v = rowKeys[i];
                nodes.Add(new JObject
                {
                    ["name"] = rowKeys[i].ToString(),
                    ["index"] = i,
                    ["group"] = vertexToGroup.TryGetValue(v, out var g) ? g : 1
                });
            }

            var m = dsm.DsmMatrixView;
            for (int r = 0; r < m.RowCount; r++)
                for (int c = 0; c < m.ColumnCount; c++)
                {
                    if (m[r, c] != 0)
                    {
                        edges.Add(new JObject
                        {
                            ["source"] = r,
                            ["target"] = c
                            // при желании можно добавить ["group"] = nodes[r]["group"]
                        });
                    }
                }

            return (nodes, edges);
        }

        /// <summary>
        /// Формирует данные для reorderable-матрицы в формате Vega (v6).
        /// </summary>
        // public static JObject ToVegaReorderableMatrix(this IDsm dsm,
        //                                               List<List<PSVertex>> partitions)
        // {
        //     var nodeValues = new JArray();
        //     var edgeValues = new JArray();

        //     /* ------------------------ 1.  Сопоставляем вершину → группа ------------------------ */
        //     var vertexToGroup = new Dictionary<PSVertex, int>();

        //     // Группы нумеруем с 1; «непомеченные» вершины получат группу 0.
        //     if (partitions is { Count: > 0 })
        //     {
        //         for (int gi = 0; gi < partitions.Count; gi++)
        //             foreach (var v in partitions[gi])
        //                 vertexToGroup[v] = gi + 1;
        //     }

        //     /* ------------------------ 2.  Узлы (nodes.values) ---------------------------------- */
        //     var rowKeys = dsm.RowIndex
        //                      .OrderBy(kvp => kvp.Value)
        //                      .Select(kvp => kvp.Key)
        //                      .ToList();

        //     for (int i = 0; i < rowKeys.Count; i++)
        //     {
        //         var v = rowKeys[i];
        //         nodeValues.Add(new JObject
        //         {
        //             ["name"] = v.ToString(),
        //             ["index"] = i,
        //             ["group"] = vertexToGroup.TryGetValue(v, out var g) ? g : 0
        //         });
        //     }

        //     /* ------------------------ 3.  Рёбра (edges.values) --------------------------------- */
        //     var m = dsm.DsmMatrixView;

        //     for (int r = 0; r < m.RowCount; r++)
        //         for (int c = 0; c < m.ColumnCount; c++)
        //         {
        //             if (m[r, c] == 0) continue;

        //             // определяем, внутри одной группы или межгрупповое ребро
        //             var srcGroup = (int)nodeValues[r]["group"];
        //             var dstGroup = (int)nodeValues[c]["group"];
        //             var edgeGroup = srcGroup == dstGroup ? srcGroup : -1;

        //             edgeValues.Add(new JObject
        //             {
        //                 ["source"] = r,
        //                 ["target"] = c,
        //                 ["group"] = edgeGroup    // пригодится, если захотите подсвечивать связи
        //                                          // ["weight"] = m[r, c]   // при необходимости можно добавить вес
        //             });
        //         }

        //     /* ------------------------ 4.  Итоговый объект для Vega ----------------------------- */
        //     return new JObject
        //     {
        //         ["nodes"] = new JObject { ["values"] = nodeValues },
        //         ["edges"] = new JObject { ["values"] = edgeValues }
        //     };
        // }

    }
}
