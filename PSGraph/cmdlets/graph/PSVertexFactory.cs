using System.Management.Automation;
using PSGraph.Model;

namespace PSGraph.Cmdlets;

internal static class PSVertexFactory
{
    public static PSVertex FromPSObject(PSObject value)
    {
        if (value.ImmediateBaseObject is PSVertex vertex)
        {
            return vertex;
        }

        var source = value;
        return new PSVertex(GetLabel(value, source), source);
    }

    private static string GetLabel(PSObject value, object? source)
    {
        var name = value.Properties["Name"]?.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return source?.ToString() ?? string.Empty;
    }
}
