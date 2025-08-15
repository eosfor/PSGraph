---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Add-Vertex

## SYNOPSIS
Add a vertex to a graph (no-op if an equivalent vertex already exists).

## SYNTAX

```
Add-Vertex -Vertex <PSObject>
 -Graph <QuikGraph.IMutableVertexAndEdgeListGraph`2[PSGraph.Model.PSVertex,PSGraph.Model.PSEdge]>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Adds a vertex object into the provided graph. If you pass an existing PSVertex it is added
directly; otherwise the object is wrapped in a new PSVertex whose Label is the object's ToString().
Duplicate detection relies on PSVertex equality (label based) so calling multiple times with the
same label will not create duplicates. The cmdlet writes no output; inspect the graph instance.

## EXAMPLES

### Example 1
Add three plain string vertices.
```powershell
$g = New-Graph
Add-Vertex -Vertex A -Graph $g
Add-Vertex -Vertex B -Graph $g
Add-Vertex -Vertex C -Graph $g
$g.VertexCount  # 3
```

### Example 2
Add a pre-constructed PSVertex to preserve metadata.
```powershell
$g = New-Graph
$v = [PSGraph.Model.PSVertex]::new("ServiceA")
Add-Vertex -Vertex $v -Graph $g
```

## PARAMETERS

### -Graph
Graph instance receiving the vertex.

```yaml
Type: QuikGraph.IMutableVertexAndEdgeListGraph`2[PSGraph.Model.PSVertex,PSGraph.Model.PSEdge]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Vertex
Object or PSVertex to add. Non-PSVertex objects are wrapped automatically.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Internal PowerShell progress preference (not commonly used).

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

### None
## NOTES

## RELATED LINKS
Add-Edge
New-Graph
