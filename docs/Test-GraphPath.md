---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Test-GraphPath

## SYNOPSIS
Return True if any directed path exists from one vertex to another.

## SYNTAX

```
Test-GraphPath -From <PSVertex> -To <PSVertex> -Graph <PsBidirectionalGraph>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Performs a reachability test between two vertices. If the vertices are equal returns True. If they
are in the same strongly connected component returns True. Otherwise it builds a condensation
component adjacency and breadth‑first searches for a path of components from source to target. If
either vertex is not in the graph returns False. Outputs a single Boolean value.

## EXAMPLES

### Example 1
Basic reachability.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Add-Edge -From B -To C -Graph $g
Test-GraphPath -From ([PSGraph.Model.PSVertex]::new('A')) -To ([PSGraph.Model.PSVertex]::new('C')) -Graph $g  # True
Test-GraphPath -From ([PSGraph.Model.PSVertex]::new('C')) -To ([PSGraph.Model.PSVertex]::new('A')) -Graph $g  # False
```

### Example 2
Cycle detection counts as reachable.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Add-Edge -From B -To A -Graph $g
Test-GraphPath -From ([PSGraph.Model.PSVertex]::new('A')) -To ([PSGraph.Model.PSVertex]::new('B')) -Graph $g  # True
```

## PARAMETERS

### -From
Source vertex.

```yaml
Type: PSVertex
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Graph
Graph to test.

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

### -To
Destination vertex.

```yaml
Type: PSVertex
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

### System.Boolean
True if a path exists, otherwise False.
## NOTES

## RELATED LINKS
