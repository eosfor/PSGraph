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

    [Parameter(Mandatory = false)]
    public ScriptBlock? GraphScript { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? VertexScript { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? EdgeScript { get; set; }

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
        ApplyScriptProperties(
            GraphScript,
            Graph,
            graphviz.GraphFormat,
            new PSVariable(nameof(Graph), Graph));
        graphviz.FormatVertex += Graphviz_FormatVertex;
        graphviz.FormatEdge += Graphviz_FormatEdge;
        return graphviz.Generate();
    }

    private void Graphviz_FormatVertex(object sender, FormatVertexEventArgs<PSVertex> args)
    {
        ApplyRenderProperties(args.Vertex.RenderProperties, args.VertexFormat);
        ApplyScriptProperties(
            VertexScript,
            args.Vertex.OriginalObject ?? args.Vertex,
            args.VertexFormat,
            new PSVariable("Vertex", args.Vertex),
            new PSVariable(nameof(Graph), Graph));
    }

    private void Graphviz_FormatEdge(object sender, FormatEdgeEventArgs<PSVertex, PSEdge> args)
    {
        ApplyRenderProperties(args.Edge.RenderProperties, args.EdgeFormat);
        ApplyScriptProperties(
            EdgeScript,
            args.Edge,
            args.EdgeFormat,
            new PSVariable("Edge", args.Edge),
            new PSVariable("Source", args.Edge.Source),
            new PSVariable("Target", args.Edge.Target),
            new PSVariable(nameof(Graph), Graph));
    }

    private static void ApplyRenderProperties(IDictionary<string, object?> properties, object formatTarget)
    {
        foreach (var entry in properties)
        {
            var propertyName = NormalizeRenderPropertyName(entry.Key);
            var destProperty = formatTarget.GetType().GetProperty(
                propertyName,
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

    private static void ApplyScriptProperties(
        ScriptBlock? script,
        object dollarUnder,
        object formatTarget,
        params PSVariable[] variables)
    {
        if (script is null)
        {
            return;
        }

        var properties = InvokeAttributeScript(script, dollarUnder, variables);
        ApplyRenderProperties(properties, formatTarget);
    }

    private static IDictionary<string, object?> InvokeAttributeScript(
        ScriptBlock script,
        object dollarUnder,
        params PSVariable[] variables)
    {
        var contextVariables = new List<PSVariable>
        {
            new("_", dollarUnder),
            new("PSItem", dollarUnder),
            new("InputObject", dollarUnder),
            new("Object", dollarUnder)
        };
        contextVariables.AddRange(variables);

        var output = script.InvokeWithContext(
            functionsToDefine: null,
            variablesToDefine: contextVariables,
            args: new[] { dollarUnder });

        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in output)
        {
            AddScriptOutputProperties(properties, item);
        }

        return properties;
    }

    private static void AddScriptOutputProperties(IDictionary<string, object?> properties, PSObject? item)
    {
        if (item is null)
        {
            return;
        }

        var value = item.BaseObject;
        if (value is null)
        {
            return;
        }

        if (value is System.Collections.IDictionary dictionary)
        {
            foreach (System.Collections.DictionaryEntry entry in dictionary)
            {
                if (entry.Key is not null)
                {
                    properties[entry.Key.ToString()!] = entry.Value;
                }
            }

            return;
        }

        if (value is System.Collections.DictionaryEntry dictionaryEntry)
        {
            if (dictionaryEntry.Key is not null)
            {
                properties[dictionaryEntry.Key.ToString()!] = dictionaryEntry.Value;
            }

            return;
        }

        foreach (var property in item.Properties)
        {
            if (!property.IsGettable)
            {
                continue;
            }

            properties[property.Name] = property.Value;
        }
    }

    private static string NormalizeRenderPropertyName(string propertyName)
    {
        return propertyName.ToLowerInvariant() switch
        {
            "rankdir" => "RankDirection",
            _ => propertyName
        };
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

        if (effectiveTargetType == typeof(GraphvizEdgeLabel))
        {
            if (value is string edgeLabel)
            {
                convertedValue = new GraphvizEdgeLabel { Value = edgeLabel };
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
