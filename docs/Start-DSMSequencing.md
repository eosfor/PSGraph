---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Start-DSMSequencing

## SYNOPSIS
Produce a new DSM with vertices re-ordered to surface sources, strongly connected components (loops) and sinks.

## SYNTAX

```
Start-DSMSequencing -Dsm <IDsm> [-LoopDetectionMethod <LoopDetectionMethod>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Runs the DSM sequencing algorithm over the supplied DSM. Sequencing iteratively peels off source and sink vertices, collapsing strongly connected components (SCC) of the remaining core using the selected loop detection method. The resulting order groups cycles and moves them between initial sources and final sinks.

Two loop detection strategies are available:
* Condensation (default) – builds a condensed graph (SCC DAG) each iteration.
* Powers – (experimental) alternative detection via adjacency powers.

Returns a new DSM instance whose `RowIndex` / `ColIndex` reflect the new ordering.

## EXAMPLES

### Example 1
Sequence a DSM with default (condensation) loop detection.
```powershell
$seq = Start-DSMSequencing -Dsm $dsm
$seq.RowIndex.Keys | ForEach-Object Label
```

### Example 2
Use powers-based loop detection.
```powershell
$seq = Start-DSMSequencing -Dsm $dsm -LoopDetectionMethod Powers
```

### Example 3
Pipe result to export.
```powershell
$seq = Start-DSMSequencing -Dsm $dsm
Export-DSM -SequencedDsm $seq -Format VEGA_HTML -Path $env:TEMP/dsmSequenced.html
```

## PARAMETERS

### -Dsm
DSM to sequence (output of `New-DSM` or a clustered DSM).

```yaml
Type: IDsm
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoopDetectionMethod
Strategy for identifying strongly connected components (cycles). Default: Condensation.

```yaml
Type: LoopDetectionMethod
Parameter Sets: (All)
Aliases:
Accepted values: Powers, Condensation

Required: False
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

### System.Object
## NOTES

## RELATED LINKS
