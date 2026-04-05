---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Import-Graph

## SYNOPSIS
Import a graph from GraphML, CSV, JSON, or Matrix Market into a new PsBidirectionalGraph.

## SYNTAX

```
Import-Graph -Path <String> [-Format <GraphImportTypes>] [-FromColumn <String>] [-ToColumn <String>] [-Delimiter <Char>] [-NoHeader] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Creates a new empty PsBidirectionalGraph and populates it from the specified file.
Supported formats:

- **GraphML** (default) - standard GraphML XML interchange format.
- **Csv** - edge-list CSV/TSV. The first row is treated as a header unless `-NoHeader` is specified.
  Source and target columns default to `From` and `To`, and can be changed with `-FromColumn` and
  `-ToColumn`. In headerless mode the first two columns are used positionally. Lines starting with
  `#` are skipped as comments. Use `-Delimiter` to change the field separator.
- **Json** - JSON object with `nodes` and `edges` or `links` arrays. Nodes can use `id`, `label`,
  or `name`. Edges can use `source` or `from`, and `target` or `to`. D3-style numeric node indexes
  are supported.
- **MatrixMarket** - Matrix Market coordinate format (`.mtx`) for sparse graph edge lists. Comment
  lines starting with `%` are ignored.

Vertices are deduplicated by label during import.

GraphML remains the neutral interchange format for `PSGraph`. It stays in this repo even though
visualization renderers have moved to `PSGraphView`, because GraphML is not renderer-specific.

## EXAMPLES

### Example 1
Load a previously exported GraphML file.
```powershell
$g = Import-Graph -Path ./graph.graphml
$g.VertexCount
$g.EdgeCount
```

### Example 2
Import a CSV edge list.
```powershell
$g = Import-Graph -Path ./edges.csv -Format Csv
$g.Vertices | Select-Object Label
```

### Example 3
Import JSON data.
```powershell
$g = Import-Graph -Path ./graph.json -Format Json
$g.Edges | ForEach-Object { "$($_.Source) -> $($_.Target)" }
```

### Example 4
Import a Matrix Market dataset.
```powershell
$g = Import-Graph -Path ./soc-karate.mtx -Format MatrixMarket
$g.VertexCount
```

## PARAMETERS

### -Path
Path to the file to import.

```yaml
Type: String
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

### -Format
Import format. Valid values: `GraphML` (default), `Csv`, `Json`, `MatrixMarket`.

```yaml
Type: GraphImportTypes
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: GraphML
Accept pipeline input: False
Accept wildcard characters: False
```

### -FromColumn
Name of the CSV column containing source vertex labels. Default: `From`.
Only used when `-Format Csv`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: From
Accept pipeline input: False
Accept wildcard characters: False
```

### -ToColumn
Name of the CSV column containing target vertex labels. Default: `To`.
Only used when `-Format Csv`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: To
Accept pipeline input: False
Accept wildcard characters: False
```

### -Delimiter
Field separator character for CSV import. Default: `,`.
Use `"`t"` for tab-separated files. Only used when `-Format Csv`.

```yaml
Type: Char
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: ,
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoHeader
When specified, the CSV file is treated as headerless. The first column is used as
the source vertex and the second column as the target vertex. `-FromColumn` and `-ToColumn`
are ignored. Only used when `-Format Csv`.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### PSGraph.Model.PsBidirectionalGraph
The imported graph.
## NOTES

## RELATED LINKS
