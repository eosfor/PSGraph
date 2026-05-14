using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;

namespace PSGraph.Model;

public sealed class PSVertex : IComparable<PSVertex>
{
    private static int nextUniqueId;
    private int uniqueId;
    private PSVertexIdentityKind identityKind = PSVertexIdentityKind.Label;
    private string label = string.Empty;
    public int Id => uniqueId;
    public string Name => Label;

    public string Label
    {
        get => label;
        set => SetLabel(value ?? throw new ArgumentNullException(nameof(value)));
    }

    public object? OriginalObject;
    public IDictionary<string, object?> RenderProperties { get; set; } = new ExpandoObject();
    public IDictionary<string, object?> Metadata { get; set; } = new ExpandoObject();

    private void SetLabel(string value)
    {
        label = value;
        RenderProperties["Label"] = value;
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

        RenderProperties = CloneProperties(other.RenderProperties);

        // TODO: оригинальный объект передается снаружи. точно не известно какой он. пока так.
        OriginalObject = other.OriginalObject is ICloneable c
            ? c.Clone()
            : other.OriginalObject;

        // Копия метаданных
        Metadata = CloneProperties(other.Metadata);

        // Label через сеттер поддерживает согласованность RenderProperties["Label"].
        Label = other.Label;

        identityKind = other.identityKind;
        uniqueId = other.uniqueId;
    }

    internal bool UsesUniqueIdentity => identityKind == PSVertexIdentityKind.Unique;

    internal void EnsureUniqueIdentity()
    {
        if (identityKind == PSVertexIdentityKind.Unique)
        {
            return;
        }

        uniqueId = Interlocked.Increment(ref nextUniqueId);
        identityKind = PSVertexIdentityKind.Unique;
    }

    private static IDictionary<string, object?> CloneProperties(IDictionary<string, object?> src)
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
    {
        if (other is null)
        {
            return 1;
        }

        var labelComparison = StringComparer.Ordinal.Compare(this.Label, other.Label);
        if (labelComparison != 0)
        {
            return labelComparison;
        }

        if (identityKind != PSVertexIdentityKind.Unique && other.identityKind != PSVertexIdentityKind.Unique)
        {
            return 0;
        }

        var identityKindComparison = identityKind.CompareTo(other.identityKind);
        return identityKindComparison != 0
            ? identityKindComparison
            : uniqueId.CompareTo(other.uniqueId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PSVertex v)
        {
            return false;
        }

        if (identityKind == PSVertexIdentityKind.Unique || v.identityKind == PSVertexIdentityKind.Unique)
        {
            return identityKind == v.identityKind && uniqueId == v.uniqueId;
        }

        return StringComparer.Ordinal.Equals(this.Label, v.Label);
    }

    public override int GetHashCode()
        => identityKind == PSVertexIdentityKind.Unique
            ? HashCode.Combine(identityKind, uniqueId)
            : StringComparer.Ordinal.GetHashCode(this.Label);

    public override string ToString() => Label;

    private enum PSVertexIdentityKind
    {
        Label,
        Unique
    }
}
