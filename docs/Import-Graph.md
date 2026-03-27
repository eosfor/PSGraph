---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Import-Graph

## SYNOPSIS
Import a graph from a GraphML, CSV, or JSON file into a new PsBidirectionalGraph.

## SYNTAX

```
Import-Graph -Path <String> [-Format <GraphImportTypes>] [-FromColumn <String>] [-ToColumn <String>] [-Delimiter <Char>] [-NoHeader] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Creates a new empty PsBidirectionalGraph and populates it from the specified file.
Supported formats:

- **GraphML** (default) -- standard GraphML XML interchange format.
- **Csv** -- edge-list CSV/TSV. First row is a header unless `-NoHeader` is specified;
  requires columns for source and target vertices (default names `From` and `To`,
  customizable via `-FromColumn`/`-ToColumn`). In headerless mode the first two columns
  are used positionally. Optional columns: `Label`, `Weight`. All other columns are
  stored as edge metadata (RenderProperties). Lines starting with `#` are treated as
  comments and skipped. Use `-Delimiter` to change the field separator (default `,`).
- **Json** -- JSON object with `nodes` and/or `edges`/`links` arrays.
  Each node needs an `id`, `label`, or `name` property; extra properties go to vertex Metadata.
  Each edge needs `source`/`from` and `target`/`to` (string labels or numeric node-array indices);
  optional `label` and `weight`; extra properties go to edge RenderProperties.
  Compatible with D3.js graph datasets (e.g. Les Miserables).

Vertices are automatically deduplicated by label.

## EXAMPLES

### Example 1: Import GraphML
```powershell
$g = Import-Graph -Path ./graph.graphml
$g.VertexCount
$g.EdgeCount
```

### Example 2: Import CSV edge list
```powershell
$g = Import-Graph -Path ./edges.csv -Format Csv
$g.Vertices | Select-Object Label
```

### Example 3: Import CSV with custom column names
```powershell
$g = Import-Graph -Path ./data.csv -Format Csv -FromColumn Source -ToColumn Target
```

### Example 4: Import JSON
```powershell
$g = Import-Graph -Path ./graph.json -Format Json
$g.Edges | ForEach-Object { "$($_.Source) -> $($_.Target)" }
```

### Example 5: Import SNAP-style TSV (tab-separated, no header, comments)
```powershell
$g = Import-Graph -Path ./web-Google.txt -Format Csv -Delimiter "`t" -NoHeader
$g.VertexCount
```

### Example 6: Import D3.js Les Miserables JSON
```powershell
$g = Import-Graph -Path ./miserables.json -Format Json
$g.Vertices | Select-Object Label, @{N='Group'; E={ $_.Metadata['group'] }}
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

### -Format
Import format. Valid values: `GraphML` (default), `Csv`, `Json`.

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
the source vertex and the second column as the target vertex. `-FromColumn`/`-ToColumn`
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

### PSGraph.Model.PsBidirectionalGraph
The imported graph.
## NOTES

## RELATED LINKS
