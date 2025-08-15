---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Start-DSMClustering

## SYNOPSIS
Partition (cluster) a DSM using classic simulated annealing or graph-based algorithm.

## SYNTAX

```
Start-DSMClustering [-AlgorithmConfig <PSObject>] -Dsm <IDsm>
 [-ClusteringAlgorithm <ClusteringAlgorithmOptions>] [-Detailed] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Runs a clustering / partitioning algorithm over the DSM to group related vertices (modules). Two algorithms are supported:

* Classic – simulated annealing heuristic (`DsmSimulatedAnnealingAlgorithm`). Accepts temperature / cooling parameters via `-AlgorithmConfig`.
* GraphBased – graph partitioning heuristic (`DsmGraphPartitioningAlgorithm`).

`Start-DSMClustering` returns either:
* A `PartitioningResult` (default) containing the resulting DSM plus the algorithm (with its `Partitions` list), or
* An extended details object when `-Detailed` is used (includes metrics / intermediate scores).

You can create a config object in several ways: strongly-typed record instance, hashtable, or PSCustomObject; the cmdlet will coerce it.

## EXAMPLES

### Example 1
Run classic clustering with a hashtable config and get basic result.
```powershell
$cfg = @{ InitialTemperature = 5; CoolingRate = 0.9 }
$res = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm Classic -AlgorithmConfig $cfg
```

### Example 2
Get detailed output for graph-based clustering.
```powershell
$detail = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm GraphBased -Detailed
```

### Example 3
Export clustered DSM to Vega HTML.
```powershell
$res = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm Classic
Export-DSM -Result $res -Format VEGA_HTML -Path $env:TEMP/dsmClustered.html
```

## PARAMETERS

### -AlgorithmConfig
Optional configuration object (Hashtable / PSCustomObject / typed config) converted to the algorithm's config type.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClusteringAlgorithm
Selects clustering algorithm: Classic (simulated annealing) or GraphBased.

```yaml
Type: ClusteringAlgorithmOptions
Parameter Sets: (All)
Aliases:
Accepted values: Classic, GraphBased

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Detailed
Switch to return extended result details instead of a simple `PartitioningResult`.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Dsm
DSM to cluster.

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
