---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# New-Graph

## SYNOPSIS
Create a new directed bidirectional graph suitable for adding vertices and edges.

## SYNTAX

```
New-Graph [-UseNonUniqueLabels] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Creates an empty PsBidirectionalGraph (directed) which supports vertex/edge list operations and
can be exported or transformed into a DSM. Use Add-Vertex / Add-Edge to populate it. All further
graph algorithms/cmdlets accept this graph type.

By default, vertices with the same label are treated as the same graph vertex. Use
`-UseNonUniqueLabels` when multiple distinct vertices may have the same display label, such as
operator nodes in expression graphs.

## EXAMPLES

### Example 1
Create a graph and add an edge.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
$g.VertexCount  # 2
$g.EdgeCount    # 1
```

### Example 2
Create a graph where labels do not have to be unique.
```powershell
$g = New-Graph -UseNonUniqueLabels
$plus1 = Add-Vertex -Vertex '+' -Graph $g -PassThru
$plus2 = Add-Vertex -Vertex '+' -Graph $g -PassThru
$g.VertexCount  # 2
```

## PARAMETERS

### -UseNonUniqueLabels
Allow multiple distinct vertices to have the same Label. In this mode, pass PSVertex objects
returned by `Add-Vertex -PassThru` to `Add-Edge` when you need to target a specific existing
vertex.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Internal PowerShell progress preference.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### PSGraph.Model.PsBidirectionalGraph
The new empty graph.
## NOTES

## RELATED LINKS
