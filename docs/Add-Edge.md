---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Add-Edge

## SYNOPSIS
Add a directed edge between two vertices (creating the vertices if they do not yet exist).

## SYNTAX

```
Add-Edge -From <PSObject> -To <PSObject>
 -Graph <QuikGraph.IMutableVertexAndEdgeListGraph`2[PSGraph.Model.PSVertex,PSGraph.Model.PSEdge]>
 [-Tag <Object>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Adds a directed edge From -> To into the supplied mutable graph. If either -From or -To is not
already a PSGraph PSVertex it is wrapped in a new PSVertex whose Label is the object's ToString().
If the vertices already exist (label equality) the existing instances are reused. An optional
Tag value is stored on the created PSEdge (converted to string when present). The cmdlet does not
emit a value; use the graph object to inspect results.

## EXAMPLES

### Example 1
Create a graph and add an edge A -> B (vertices auto-created).
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
$g.EdgeCount   # 1
```

### Example 2
Attach a tag to an edge.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g -Tag Dependency
($g.Edges | Select-Object -First 1).Tag.Value  # "Dependency"
```

## PARAMETERS

### -From
Source vertex (or arbitrary object which will be wrapped as a vertex) for the edge.

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

### -Graph
Graph instance receiving the edge. Must be a mutable, directed QuikGraph implementation.

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

### -Tag
Optional label stored inside the created edge's Tag object (stringified).

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -To
Target vertex (or object to wrap) for the edge.

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
Internal PowerShell progress preference (normally not used).

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
Add-Vertex
New-Graph
