---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Get-OutEdge

## SYNOPSIS
List outgoing edges for a specified vertex.

## SYNTAX

```
Get-OutEdge -Vertex <PSVertex> -Graph <PsBidirectionalGraph> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Returns all edges whose Source is the supplied vertex (if any). Writes nothing when the vertex has
no outgoing edges. Vertex identity is label based.

## EXAMPLES

### Example 1
List edges leaving A.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Add-Edge -From A -To C -Graph $g
Get-OutEdge -Vertex ([PSGraph.Model.PSVertex]::new('A')) -Graph $g
```

## PARAMETERS

### -Graph
Graph containing the vertex.

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

### -Vertex
Source vertex whose outgoing edges will be returned.

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

### PSGraph.Model.PSEdge
Outgoing edge objects (enumerated) when present.
## NOTES

## RELATED LINKS
