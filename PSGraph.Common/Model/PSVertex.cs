using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using QuikGraph.Graphviz.Dot;

namespace PSGraph.Model;

public sealed class PSVertex : IComparable<PSVertex>
{
    private readonly Guid id = Guid.NewGuid();
    private string label = string.Empty;
    public string Name => Label;

    public string Label
    {
        get => label;
        set => SetLabel(value ?? throw new ArgumentNullException(nameof(value)));
    }

    public GraphvizVertex GVertexParameters = new GraphvizVertex();
    public object? OriginalObject;
    public IDictionary<string, object?> Metadata { get; set; } = new ExpandoObject();

    private void SetLabel(string value)
    {
        label = value;
        GVertexParameters.Label = value;
    }

    public PSVertex(string label) => Label = label;

    public PSVertex(string label, object source)
    {
        Label = label;
        OriginalObject = source;
    }

    // Копирующий конструктор
    public PSVertex(PSVertex other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));

        // Клонируем параметры отрисовки
        GVertexParameters = CloneGraphvizVertex(other.GVertexParameters);

        // TODO: оригинальный объект передается снаружи. точно не известно какой он. пока так.
        OriginalObject = other.OriginalObject is ICloneable c
            ? c.Clone()
            : other.OriginalObject;

        // Копия метаданных
        Metadata = CloneMetadata(other.Metadata);

        // Label через сеттер (поддержит согласованность с GVertexParameters.Label)
        Label = other.Label;
    }

    private static GraphvizVertex CloneGraphvizVertex(GraphvizVertex src)
    {
        if (src is null) return new GraphvizVertex();
        var dst = new GraphvizVertex();
        foreach (var p in typeof(GraphvizVertex).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.CanRead && p.CanWrite)
            {
                var val = p.GetValue(src);
                p.SetValue(dst, val);
            }
        }
        return dst;
    }

    private static IDictionary<string, object?> CloneMetadata(IDictionary<string, object?> src)
    {
        var expando = new ExpandoObject();
        var dst = (IDictionary<string, object?>)expando;
        foreach (var kv in src)
        {
            dst[kv.Key] = kv.Value is ICloneable c ? c.Clone() : kv.Value;
        }
        return dst;
    }

    public static bool operator ==(PSVertex? left, PSVertex? right)
    => Equals(left, right);

    public static bool operator !=(PSVertex? left, PSVertex? right)
        => !Equals(left, right);

    public int CompareTo(PSVertex? other)
        => other is null ? 1 : StringComparer.Ordinal.Compare(this.Label, other.Label);

    public override bool Equals(object? obj)
        => obj is PSVertex v && StringComparer.Ordinal.Equals(this.Label, v.Label);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(this.Label);

    public override string ToString() => Label;
}