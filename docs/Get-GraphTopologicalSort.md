---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Get-GraphTopologicalSort

## SYNOPSIS
Return vertices from a directed acyclic graph in topological order.

## SYNTAX

```
Get-GraphTopologicalSort -Graph <PsBidirectionalGraph> [-Reverse] [-StartVertex <PSVertex>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
`Get-GraphTopologicalSort` returns every vertex in a directed acyclic graph (DAG) so that each
edge source appears before its target. This is useful for dependency graphs, build graphs,
execution ordering, and expression graphs.

Topological order is not necessarily unique. Independent vertices may appear in different valid
positions as long as every edge preserves the source-before-target constraint.

If the graph contains a cycle, the cmdlet writes a terminating error. Use DSM sequencing or
condensation algorithms when cyclic components need to be grouped before ordering.

When `-StartVertex` is specified, the cmdlet sorts only the vertices reachable from that vertex
by outgoing edges. Cycles outside the reachable subgraph do not affect the result.

## EXAMPLES

### Example 1
Sort a dependency graph.

```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g | Out-Null
Add-Edge -From A -To C -Graph $g | Out-Null
Add-Edge -From B -To D -Graph $g | Out-Null
Add-Edge -From C -To D -Graph $g | Out-Null

Get-GraphTopologicalSort -Graph $g | ForEach-Object Name
```

`A` appears before `B` and `C`; both `B` and `C` appear before `D`.

### Example 2
Return targets before sources.

```powershell
$g = New-Graph
Add-Edge -From Compile -To Test -Graph $g | Out-Null
Add-Edge -From Test -To Package -Graph $g | Out-Null

Get-GraphTopologicalSort -Graph $g -Reverse | ForEach-Object Name
```

### Example 3
Sort only the reachable subgraph from a start vertex.

```powershell
$g = New-Graph
$a = Add-Vertex -Vertex A -Graph $g
$b = Add-Vertex -Vertex B -Graph $g
$c = Add-Vertex -Vertex C -Graph $g
$x = Add-Vertex -Vertex X -Graph $g

Add-Edge -From $a -To $b -Graph $g | Out-Null
Add-Edge -From $b -To $c -Graph $g | Out-Null

Get-GraphTopologicalSort -Graph $g -StartVertex $b | ForEach-Object Name
```

The command returns `B`, then `C`. The independent vertex `X` is not emitted.

### Example 4
Cycles are rejected.

```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g | Out-Null
Add-Edge -From B -To A -Graph $g | Out-Null

Get-GraphTopologicalSort -Graph $g
```

The cmdlet fails because a graph with a cycle has no topological ordering.

## PARAMETERS

### -Graph
Directed graph to sort. The graph must be acyclic.

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

### -StartVertex
Optional vertex that limits the sort to the subgraph reachable from that vertex by outgoing edges.

```yaml
Type: PSVertex
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Reverse
Return a valid reverse topological ordering, where targets appear before sources.

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

### PSGraph.Model.PSVertex
Every vertex in the graph, emitted in topological order.

## NOTES

## RELATED LINKS

[Get-GraphDistanceVector](Get-GraphDistanceVector.md)
[Get-GraphPath](Get-GraphPath.md)
