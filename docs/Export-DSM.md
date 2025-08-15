---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Export-DSM

## SYNOPSIS
Export a Design Structure Matrix (plain, clustered or sequenced) to text or Vega (JSON/HTML) representation.

## SYNTAX

### PlainDsm (Default)
```
Export-DSM [-Dsm] <IDsm> [[-Path] <String>] [[-Format] <DSMExportTypes>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### PartitionedDsm
```
Export-DSM [-Result] <PartitioningResult> [[-Path] <String>] [[-Format] <DSMExportTypes>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SequencedDsm
```
Export-DSM [-SequencedDsm] <IDsm> [[-Path] <String>] [[-Format] <DSMExportTypes>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Exports the contents of a DSM (`IDsm`) in one of several formats.  
You can:

* Export a plain DSM that you created with `New-DSM`.
* Export the result of clustering (`Start-DSMClustering`) by passing the returned `PartitioningResult`.
* Export a sequenced DSM returned from `Start-DSMSequencing`.

TEXT format outputs a simple adjacency matrix (comma separated rows).  
VEGA_JSON / VEGA_HTML embed node & edge data into a Vega specification (matrix view) suitable for visualization (HTML wraps the spec).

## EXAMPLES

### Example 1
Export a plain DSM to text (matrix) file.
```powershell
$dsm = New-DSM -Graph (New-Graph)
# ... add vertices & edges, then:
Export-DSM -Dsm $dsm -Format TEXT -Path $env:TEMP/dsm.txt
```

### Example 2
Export clustered DSM (classic algorithm) to interactive HTML Vega matrix.
```powershell
$ret = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm Classic
Export-DSM -Result $ret -Format VEGA_HTML -Path $env:TEMP/dsmClustered.html
```

### Example 3
Export sequenced DSM produced via condensation based loop detection.
```powershell
$seq = Start-DSMSequencing -Dsm $dsm -LoopDetectionMethod Condensation
Export-DSM -SequencedDsm $seq -Format VEGA_JSON > dsmSequenced.json
```

## PARAMETERS

### -Dsm
Plain DSM object to export (use when you have not clustered or sequenced yet).

```yaml
Type: IDsm
Parameter Sets: PlainDsm
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format
Output format: TEXT adjacency matrix, or Vega visualization (JSON spec only or self‑contained HTML).

```yaml
Type: DSMExportTypes
Parameter Sets: (All)
Aliases:
Accepted values: TEXT, VEGA_JSON, VEGA_HTML

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Destination file path. If omitted, the command writes the content to the pipeline.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Result
Partitioning/clustering result object returned by `Start-DSMClustering` (non-detailed mode).

```yaml
Type: PartitioningResult
Parameter Sets: PartitionedDsm
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SequencedDsm
DSM returned by `Start-DSMSequencing` (already re-ordered/loop-resolved).

```yaml
Type: IDsm
Parameter Sets: SequencedDsm
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Internal PowerShell common progress behavior override.

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
