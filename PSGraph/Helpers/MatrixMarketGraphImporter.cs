using System.Globalization;
using PSGraph.Model;

namespace PSGraph.Helpers;

internal static class MatrixMarketGraphImporter
{
    public static PsBidirectionalGraph Import(string path)
    {
        using var reader = new StreamReader(path);
        var lineNumber = 0;

        var headerLine = ReadNextNonEmptyLine(reader, ref lineNumber);
        if (headerLine is null)
            throw new InvalidDataException("Matrix Market file is empty.");

        var header = ParseHeader(headerLine);
        if (!string.Equals(header.ObjectKind, "matrix", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Matrix Market object kind '{header.ObjectKind}' is not supported.");
        if (!string.Equals(header.Format, "coordinate", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Matrix Market format '{header.Format}' is not supported. Only coordinate format is supported.");
        if (!IsSupportedField(header.Field))
            throw new InvalidDataException($"Matrix Market field '{header.Field}' is not supported. Supported fields: pattern, integer, real.");
        if (!IsSupportedSymmetry(header.Symmetry))
            throw new InvalidDataException($"Matrix Market symmetry '{header.Symmetry}' is not supported. Supported symmetries: general, symmetric.");

        var sizeLine = ReadNextDataLine(reader, ref lineNumber);
        if (sizeLine is null)
            throw new InvalidDataException("Matrix Market file is missing the size line.");

        var sizeFields = SplitFields(sizeLine);
        if (sizeFields.Length != 3)
            throw new InvalidDataException($"Matrix Market size line must contain 3 integers. Got {sizeFields.Length} fields on line {lineNumber}.");

        var rowCount = ParsePositiveInt(sizeFields[0], "row count", lineNumber);
        var columnCount = ParsePositiveInt(sizeFields[1], "column count", lineNumber);
        var entryCount = ParseNonNegativeInt(sizeFields[2], "entry count", lineNumber);

        if (rowCount != columnCount)
            throw new InvalidDataException($"Matrix Market graph import expects a square adjacency matrix, but got {rowCount}x{columnCount}.");

        var graph = new PsBidirectionalGraph(false);
        var expectsValue = !string.Equals(header.Field, "pattern", StringComparison.OrdinalIgnoreCase);

        for (var entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            var entryLine = ReadNextDataLine(reader, ref lineNumber);
            if (entryLine is null)
                throw new InvalidDataException($"Matrix Market file ended early. Expected {entryCount} entries but found {entryIndex}.");

            var entryFields = SplitFields(entryLine);
            var expectedFieldCount = expectsValue ? 3 : 2;
            if (entryFields.Length < expectedFieldCount)
                throw new InvalidDataException(
                    $"Matrix Market entry on line {lineNumber} must contain at least {expectedFieldCount} fields, got {entryFields.Length}.");

            var sourceLabel = ParsePositiveInt(entryFields[0], "row index", lineNumber).ToString(CultureInfo.InvariantCulture);
            var targetLabel = ParsePositiveInt(entryFields[1], "column index", lineNumber).ToString(CultureInfo.InvariantCulture);

            var edge = new PSEdge(new PSVertex(sourceLabel), new PSVertex(targetLabel), new PSEdgeTag());
            if (expectsValue)
            {
                ApplyValue(edge, entryFields[2], lineNumber);
            }

            graph.AddEdge(edge);
        }

        return graph;
    }

    private static void ApplyValue(PSEdge edge, string rawValue, int lineNumber)
    {
        if (!double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
            throw new InvalidDataException($"Matrix Market value '{rawValue}' on line {lineNumber} is not a valid number.");

        edge.RenderProperties["Value"] = value;

        if (value >= int.MinValue && value <= int.MaxValue && Math.Abs(value % 1) < double.Epsilon)
        {
            edge.Weight = (int)value;
        }
    }

    private static Header ParseHeader(string line)
    {
        var fields = SplitFields(line);
        if (fields.Length != 5 || !string.Equals(fields[0], "%%MatrixMarket", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid Matrix Market header.");

        return new Header(fields[1], fields[2], fields[3], fields[4]);
    }

    private static bool IsSupportedField(string field) =>
        string.Equals(field, "pattern", StringComparison.OrdinalIgnoreCase)
        || string.Equals(field, "integer", StringComparison.OrdinalIgnoreCase)
        || string.Equals(field, "real", StringComparison.OrdinalIgnoreCase);

    private static bool IsSupportedSymmetry(string symmetry) =>
        string.Equals(symmetry, "general", StringComparison.OrdinalIgnoreCase)
        || string.Equals(symmetry, "symmetric", StringComparison.OrdinalIgnoreCase);

    private static string? ReadNextNonEmptyLine(StreamReader reader, ref int lineNumber)
    {
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            lineNumber++;
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }

        return null;
    }

    private static string? ReadNextDataLine(StreamReader reader, ref int lineNumber)
    {
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith('%'))
                continue;

            return line;
        }

        return null;
    }

    private static string[] SplitFields(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static int ParsePositiveInt(string raw, string fieldName, int lineNumber)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value <= 0)
            throw new InvalidDataException($"Matrix Market {fieldName} '{raw}' on line {lineNumber} must be a positive integer.");

        return value;
    }

    private static int ParseNonNegativeInt(string raw, string fieldName, int lineNumber)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value < 0)
            throw new InvalidDataException($"Matrix Market {fieldName} '{raw}' on line {lineNumber} must be a non-negative integer.");

        return value;
    }

    private sealed record Header(string ObjectKind, string Format, string Field, string Symmetry);
}
