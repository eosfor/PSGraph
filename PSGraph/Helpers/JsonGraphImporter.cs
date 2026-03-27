using System.Globalization;
using System.Text.Json;
using PSGraph.Model;

namespace PSGraph.Helpers;

internal static class JsonGraphImporter
{
    private static readonly JsonDocumentOptions DocOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static PsBidirectionalGraph Import(string path)
    {
        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json, DocOptions);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("JSON root must be an object.");

        var graph = new PsBidirectionalGraph(false);

        // --- nodes (optional) ---
        // Build an ordered list so numeric edge references (index into array) work
        var nodeList = new List<PSVertex>();

        if (root.TryGetProperty("nodes", out var nodesElem))
        {
            if (nodesElem.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("'nodes' must be an array.");

            foreach (var node in nodesElem.EnumerateArray())
            {
                var vertex = ParseNode(node);
                graph.AddVertex(vertex);
                nodeList.Add(vertex);
            }
        }

        // --- edges: accept both "edges" and "links" (D3.js convention) ---
        JsonElement edgesElem = default;
        bool hasEdges = root.TryGetProperty("edges", out edgesElem)
                        || root.TryGetProperty("links", out edgesElem);

        if (hasEdges)
        {
            if (edgesElem.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("'edges'/'links' must be an array.");

            int idx = 0;
            foreach (var edgeElem in edgesElem.EnumerateArray())
            {
                var edge = ParseEdge(edgeElem, idx, graph, nodeList);
                graph.AddEdge(edge);
                idx++;
            }
        }

        return graph;
    }

    private static PSVertex ParseNode(JsonElement node)
    {
        string? label = GetStringProp(node, "id")
                        ?? GetStringProp(node, "label")
                        ?? GetStringProp(node, "name");
        if (string.IsNullOrEmpty(label))
            throw new InvalidDataException("Each node must have an 'id', 'label', or 'name' property.");

        var vertex = new PSVertex(label);

        // All other properties -> Metadata
        foreach (var prop in node.EnumerateObject())
        {
            var name = prop.Name;
            if (name.Equals("id", StringComparison.OrdinalIgnoreCase)
                || name.Equals("label", StringComparison.OrdinalIgnoreCase)
                || name.Equals("name", StringComparison.OrdinalIgnoreCase))
                continue;

            vertex.Metadata[name] = ElementToObject(prop.Value);
        }

        return vertex;
    }

    private static PSEdge ParseEdge(
        JsonElement edgeElem, int index, PsBidirectionalGraph graph, List<PSVertex> nodeList)
    {
        var sourceLabel = ResolveEndpoint(edgeElem, "source", "from", nodeList);
        var targetLabel = ResolveEndpoint(edgeElem, "target", "to", nodeList);

        if (string.IsNullOrEmpty(sourceLabel) || string.IsNullOrEmpty(targetLabel))
            throw new InvalidDataException(
                $"Edge at index {index}: must have 'source'/'from' and 'target'/'to' properties.");

        // Reuse existing vertex references to preserve edge properties through AddEdge
        var source = graph.Vertices.FirstOrDefault(v => v.Label == sourceLabel)
                     ?? new PSVertex(sourceLabel);
        var target = graph.Vertices.FirstOrDefault(v => v.Label == targetLabel)
                     ?? new PSVertex(targetLabel);
        var edge = new PSEdge(source, target, new PSEdgeTag());

        foreach (var prop in edgeElem.EnumerateObject())
        {
            var name = prop.Name;
            if (name.Equals("source", StringComparison.OrdinalIgnoreCase)
                || name.Equals("from", StringComparison.OrdinalIgnoreCase)
                || name.Equals("target", StringComparison.OrdinalIgnoreCase)
                || name.Equals("to", StringComparison.OrdinalIgnoreCase))
                continue;

            if (name.Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                edge.Label = prop.Value.GetString() ?? string.Empty;
                continue;
            }

            if (name.Equals("weight", StringComparison.OrdinalIgnoreCase)
                && prop.Value.TryGetInt32(out var w))
            {
                edge.Weight = w;
                continue;
            }

            // Extra properties -> edge RenderProperties as metadata
            edge.RenderProperties[name] = ElementToObject(prop.Value);
        }

        return edge;
    }

    /// <summary>
    /// Resolves an edge endpoint. If the value is an integer and a node list is available,
    /// treats it as an index into the nodes array (D3.js convention). Otherwise returns as string.
    /// </summary>
    private static string? ResolveEndpoint(
        JsonElement edgeElem, string primaryName, string altName, List<PSVertex> nodeList)
    {
        JsonElement val = default;
        bool found = false;

        // Try exact property name first, then case-insensitive
        if (edgeElem.TryGetProperty(primaryName, out val))
            found = true;
        else if (edgeElem.TryGetProperty(altName, out val))
            found = true;
        else
        {
            foreach (var prop in edgeElem.EnumerateObject())
            {
                if (prop.Name.Equals(primaryName, StringComparison.OrdinalIgnoreCase)
                    || prop.Name.Equals(altName, StringComparison.OrdinalIgnoreCase))
                {
                    val = prop.Value;
                    found = true;
                    break;
                }
            }
        }

        if (!found) return null;

        // Numeric value -> index into nodeList
        if (val.ValueKind == JsonValueKind.Number && val.TryGetInt32(out var idx))
        {
            if (nodeList.Count > 0 && idx >= 0 && idx < nodeList.Count)
                return nodeList[idx].Label;
            // If no node list or out of range, use the number as a string label
            return idx.ToString(CultureInfo.InvariantCulture);
        }

        return val.ValueKind == JsonValueKind.String
            ? val.GetString()
            : val.GetRawText();
    }

    private static string? GetStringProp(JsonElement elem, string name)
    {
        if (elem.TryGetProperty(name, out var val))
        {
            return val.ValueKind == JsonValueKind.String
                ? val.GetString()
                : val.GetRawText();
        }

        // Case-insensitive fallback
        foreach (var prop in elem.EnumerateObject())
        {
            if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString()
                    : prop.Value.GetRawText();
            }
        }

        return null;
    }

    private static object? ElementToObject(JsonElement elem) => elem.ValueKind switch
    {
        JsonValueKind.String => elem.GetString(),
        JsonValueKind.Number => elem.TryGetInt64(out var l) ? l : elem.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => elem.GetRawText()
    };
}
