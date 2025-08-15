---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# New-DSM

## SYNOPSIS
Create a classic Design Structure Matrix (DSM) view over an existing graph.

## SYNTAX

```
New-DSM -Graph <PsBidirectionalGraph> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Wraps the supplied PsBidirectionalGraph in a DsmClassic implementation providing matrix-based
operations (clustering, sequencing, export). The input graph is not modified; the DSM references
its vertices and edges to build matrix indices.

## EXAMPLES

### Example 1
Create a DSM and export to text.
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
$dsm = New-DSM -Graph $g
Export-DSM -Dsm $dsm -Format TEXT
```

## PARAMETERS

### -Graph
Underlying graph supplying vertices/edges for the DSM.

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

### PSGraph.DesignStructureMatrix.IDsm
A DSM instance (DsmClassic implementation).
## NOTES

## RELATED LINKS
