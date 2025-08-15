---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Get-GraphDistanceVector

## SYNOPSIS
Compute distance levels for all reachable vertices measured from every root (in-degree 0) vertex.

## SYNTAX

```
Get-GraphDistanceVector -Graph <PsBidirectionalGraph> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Traverses the directed graph starting from each root vertex (vertex with no incoming edges) and
records the minimal edge-distance (number of hops) to every reachable vertex. Depth-first search
is performed for each root while a distance observer accumulates levels; if multiple roots reach
the same vertex the first (lowest) distance discovered is kept. Returns a collection of
PSDistanceVectorRecord objects (Vertex, Level). Unreachable vertices that have incoming edges but
no path from any root are not emitted.

## EXAMPLES

### Example 1
Simple chain distances.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Add-Edge -From B -To C -Graph $g
Get-GraphDistanceVector -Graph $g | Sort-Object Level | Format-Table Vertex,Level
```

### Example 2
Two sources feeding a shared dependency.
```powershell
$g = New-Graph
Add-Edge -From A -To X -Graph $g
Add-Edge -From B -To X -Graph $g
Get-GraphDistanceVector -Graph $g | Sort-Object Vertex | Format-Table Vertex,Level
```

## PARAMETERS

### -Graph
Graph to analyze for root-based distances.

```yaml
Type: PsBidirectionalGraph
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
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

### PSGraph.Common.Model.PSDistanceVectorRecord
One object per reachable vertex containing its Level.
## NOTES

## RELATED LINKS
