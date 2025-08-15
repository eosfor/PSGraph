---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Get-InEdge

## SYNOPSIS
List incoming edges for a specified vertex.

## SYNTAX

```
Get-InEdge -Vertex <PSVertex> -Graph <PsBidirectionalGraph> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Returns all edges whose Target is the supplied vertex (if any). If no incoming edges exist the
cmdlet writes nothing. The vertex must be present in the graph (label equality applies if you
constructed a fresh PSVertex with the same label).

## EXAMPLES

### Example 1
Get inbound dependencies of C.
```powershell
$g = New-Graph
Add-Edge -From A -To C -Graph $g
Add-Edge -From B -To C -Graph $g
Get-InEdge -Vertex ([PSGraph.Model.PSVertex]::new('C')) -Graph $g
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
Target vertex whose incoming edges will be returned.

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
Incoming edge objects (enumerated) when present.
## NOTES

## RELATED LINKS
