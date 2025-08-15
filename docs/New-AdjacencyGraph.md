---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# New-AdjacencyGraph

## SYNOPSIS
Create a new adjacency graph (PsAdjacencyGraph) instance.

## SYNTAX

```
New-AdjacencyGraph [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Returns an empty PsAdjacencyGraph (adjacency-list backed) useful when you need that concrete
representation instead of the default PsBidirectionalGraph. Use Add-Vertex / Add-Edge to populate.

## EXAMPLES

### Example 1
Create and populate an adjacency graph.
```powershell
$g = New-AdjacencyGraph
Add-Edge -From A -To B -Graph $g
```

## PARAMETERS

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

### PSGraph.Model.PsAdjacencyGraph
The new graph instance.
## NOTES

## RELATED LINKS
