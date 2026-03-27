using System.Globalization;
using PSGraph.Model;

namespace PSGraph.Helpers;

internal static class CsvGraphImporter
{
    private static readonly StringComparer ColComparer = StringComparer.OrdinalIgnoreCase;

    public static PsBidirectionalGraph Import(
        string path, string fromColumn, string toColumn, char delimiter, bool noHeader)
    {
        var allLines = File.ReadAllLines(path);
        if (allLines.Length == 0)
            throw new InvalidDataException("CSV file is empty.");

        // Filter out comment lines (starting with #) and blank lines at the top
        var lines = allLines.Where(l => !l.StartsWith('#')).ToArray();
        if (lines.Length == 0)
            throw new InvalidDataException("CSV file contains only comments.");

        string[]? headers;
        int dataStart;
        int fromIdx;
        int toIdx;
        int labelIdx;
        int weightIdx;

        if (noHeader)
        {
            // Positional mode: col0 = From, col1 = To, no named columns
            headers = null;
            dataStart = 0;
            fromIdx = 0;
            toIdx = 1;
            labelIdx = -1;
            weightIdx = -1;
        }
        else
        {
            headers = ParseCsvLine(lines[0], delimiter);
            dataStart = 1;

            fromIdx = Array.FindIndex(headers, h => ColComparer.Equals(h, fromColumn));
            toIdx = Array.FindIndex(headers, h => ColComparer.Equals(h, toColumn));

            if (fromIdx < 0)
                throw new InvalidDataException($"Required column '{fromColumn}' not found in CSV header.");
            if (toIdx < 0)
                throw new InvalidDataException($"Required column '{toColumn}' not found in CSV header.");

            labelIdx = Array.FindIndex(headers, h => ColComparer.Equals(h, "Label"));
            weightIdx = Array.FindIndex(headers, h => ColComparer.Equals(h, "Weight"));
        }

        // Indices of extra columns that go into edge metadata
        var metaIndices = new List<int>();
        if (headers is not null)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (i != fromIdx && i != toIdx && i != labelIdx && i != weightIdx)
                    metaIndices.Add(i);
            }
        }

        var graph = new PsBidirectionalGraph(false);

        for (int lineNum = dataStart; lineNum < lines.Length; lineNum++)
        {
            var line = lines[lineNum];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line, delimiter);
            if (fields.Length <= Math.Max(fromIdx, toIdx))
                throw new InvalidDataException(
                    $"Line {lineNum + 1}: expected at least {Math.Max(fromIdx, toIdx) + 1} columns, got {fields.Length}.");

            var fromLabel = fields[fromIdx].Trim();
            var toLabel = fields[toIdx].Trim();

            if (string.IsNullOrEmpty(fromLabel) || string.IsNullOrEmpty(toLabel))
                throw new InvalidDataException(
                    $"Line {lineNum + 1}: '{fromColumn}' and '{toColumn}' must not be empty.");

            // Reuse existing vertex references so AddEdge won't create a replacement edge
            // (which would lose Label, Weight, and RenderProperties).
            var source = graph.Vertices.FirstOrDefault(v => v.Label == fromLabel)
                         ?? new PSVertex(fromLabel);
            var target = graph.Vertices.FirstOrDefault(v => v.Label == toLabel)
                         ?? new PSVertex(toLabel);
            var edge = new PSEdge(source, target, new PSEdgeTag());

            if (labelIdx >= 0 && labelIdx < fields.Length)
            {
                var lbl = fields[labelIdx].Trim();
                if (!string.IsNullOrEmpty(lbl))
                    edge.Label = lbl;
            }

            if (weightIdx >= 0 && weightIdx < fields.Length)
            {
                var wStr = fields[weightIdx].Trim();
                if (!string.IsNullOrEmpty(wStr)
                    && int.TryParse(wStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var w))
                {
                    edge.Weight = w;
                }
            }

            foreach (var mi in metaIndices)
            {
                if (mi < fields.Length)
                {
                    var val = fields[mi].Trim();
                    if (!string.IsNullOrEmpty(val))
                        edge.RenderProperties[headers![mi]] = val;
                }
            }

            graph.AddEdge(edge);
        }

        return graph;
    }

    /// <summary>
    /// Parses a single CSV/TSV line respecting double-quoted fields.
    /// </summary>
    internal static string[] ParseCsvLine(string line, char delimiter = ',')
    {
        var fields = new List<string>();
        int i = 0;

        while (i <= line.Length)
        {
            if (i == line.Length)
            {
                // trailing delimiter produced an empty final field
                if (fields.Count > 0 && i > 0 && line[i - 1] == delimiter)
                    fields.Add(string.Empty);
                break;
            }

            if (line[i] == '"')
            {
                // Quoted field
                var sb = new System.Text.StringBuilder();
                i++; // skip opening quote
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i += 2;
                        }
                        else
                        {
                            i++; // skip closing quote
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(line[i]);
                        i++;
                    }
                }
                fields.Add(sb.ToString());
                // skip delimiter after closing quote
                if (i < line.Length && line[i] == delimiter)
                    i++;
            }
            else
            {
                // Unquoted field
                int start = i;
                while (i < line.Length && line[i] != delimiter)
                    i++;
                fields.Add(line[start..i]);
                if (i < line.Length)
                    i++; // skip comma
            }
        }

        return fields.ToArray();
    }
}
