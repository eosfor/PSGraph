using System.Globalization;
using System.Management.Automation;
using System.Reflection;
using System.Xml;
using PSGraph.Model;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using QuikGraph.Serialization;

namespace PSGraph.Cmdlets;

[Cmdlet(VerbsData.Export, "Graph")]
public partial class ExportGraphViewCmdLet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public PsBidirectionalGraph Graph = null!;

    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public GraphExportTypes Format;

    [Parameter(Mandatory = false)]
    [ValidateNotNullOrEmpty]
    public string? Path;

    protected override void ProcessRecord()
    {
        string result;

        switch (Format)
        {
            case GraphExportTypes.Graphviz:
                result = ExportGraphViz();
                break;
            case GraphExportTypes.GraphML:
                result = ExportGraphMLAsString();
                break;
            default:
                ThrowTerminatingError(CreateVisualExportMovedException());
                return;
        }

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Path)))
        {
            File.WriteAllText(Path!, result);
            return;
        }

        WriteObject(result);
    }

    private string ExportGraphMLAsString()
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });

        Graph.SerializeToGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(
            xmlWriter,
            vertex => vertex.Label,
            edge => $"{edge.Source.Label}->{edge.Target.Label}");

        xmlWriter.Flush();
        return stringWriter.ToString();
    }

    private string ExportGraphViz()
    {
        var graphviz = new GraphvizAlgorithm<PSVertex, PSEdge>(Graph);
        ApplyRenderProperties(Graph.RenderProperties, graphviz.GraphFormat);
        graphviz.FormatVertex += Graphviz_FormatVertex;
        graphviz.FormatEdge += Graphviz_FormatEdge;
        return graphviz.Generate();
    }

    private void Graphviz_FormatVertex(object sender, FormatVertexEventArgs<PSVertex> args)
    {
        ApplyRenderProperties(args.Vertex.RenderProperties, args.VertexFormat);
    }

    private void Graphviz_FormatEdge(object sender, FormatEdgeEventArgs<PSVertex, PSEdge> args)
    {
        ApplyRenderProperties(args.Edge.RenderProperties, args.EdgeFormat);
    }

    private static void ApplyRenderProperties(IDictionary<string, object?> properties, object formatTarget)
    {
        foreach (var entry in properties)
        {
            var destProperty = formatTarget.GetType().GetProperty(
                entry.Key,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (destProperty is null || !destProperty.CanWrite)
            {
                continue;
            }

            if (TryConvertRenderPropertyValue(entry.Value, destProperty.PropertyType, out var convertedValue))
            {
                destProperty.SetValue(formatTarget, convertedValue);
            }
        }
    }

    private static bool TryConvertRenderPropertyValue(object? value, Type targetType, out object? convertedValue)
    {
        var effectiveTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null)
        {
            convertedValue = null;
            return !effectiveTargetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;
        }

        if (effectiveTargetType.IsInstanceOfType(value))
        {
            convertedValue = value;
            return true;
        }

        if (effectiveTargetType.IsEnum)
        {
            if (value is string text && Enum.TryParse(effectiveTargetType, text, true, out var enumValue))
            {
                convertedValue = enumValue;
                return true;
            }

            if (TryChangeType(value, Enum.GetUnderlyingType(effectiveTargetType), out var numericValue))
            {
                convertedValue = Enum.ToObject(effectiveTargetType, numericValue!);
                return true;
            }

            convertedValue = null;
            return false;
        }

        if (effectiveTargetType == typeof(GraphvizColor))
        {
            if (value is GraphvizColor graphvizColor)
            {
                convertedValue = graphvizColor;
                return true;
            }

            convertedValue = null;
            return false;
        }

        if (TryChangeType(value, effectiveTargetType, out convertedValue))
        {
            return true;
        }

        convertedValue = null;
        return false;
    }

    private static bool TryChangeType(object value, Type targetType, out object? convertedValue)
    {
        try
        {
            convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            convertedValue = null;
            return false;
        }
    }

    private ErrorRecord CreateVisualExportMovedException()
    {
        return new ErrorRecord(
            new NotSupportedException($"Visual export format '{Format}' is no longer provided by PSGraph. Use PSGraphView and Export-GraphView instead."),
            "VisualExportMovedToPSGraphView",
            ErrorCategory.NotImplemented,
            Format);
    }
}
