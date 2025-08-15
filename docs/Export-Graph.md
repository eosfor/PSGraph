---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Export-Graph

## SYNOPSIS
Export a graph to Graphviz DOT, GraphML, MSAGL SVG layout, or Vega JSON/HTML visualizations.

## SYNTAX

```
Export-Graph -Graph <PsBidirectionalGraph> -Format <GraphExportTypes> [-Path <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Exports a PsBidirectionalGraph to one of multiple supported formats for analysis or visualization:

* Graphviz – DOT text (layout performed later by graphviz tools).
* GraphML – XML interchange format.
* MSAGL_* – Uses Microsoft Automatic Graph Layout to compute positions and returns SVG.
* Vega_* – Emits data-driven Vega specs (force directed, adjacency matrix, or tree); choose JSON or HTML by file extension (.json or .html).

If -Path is supplied the content is written to that file; otherwise the string is written to the pipeline.
File extension also influences Vega export type: .html produces self-contained HTML; .json returns just the spec.

## EXAMPLES

### Example 1
Export to Graphviz DOT (string to pipeline).
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Export-Graph -Graph $g -Format Graphviz | Out-File graph.dot
```

### Example 2
Generate an MSAGL MDS SVG file.
```powershell
$g = New-Graph; Add-Edge -From A -To B -Graph $g; Add-Edge -From B -To C -Graph $g
Export-Graph -Graph $g -Format MSAGL_MDS -Path graph.svg
```

### Example 3
Produce an interactive force-directed HTML (Vega).
```powershell
$g = New-Graph; Add-Edge -From A -To B -Graph $g; Add-Edge -From A -To C -Graph $g
Export-Graph -Graph $g -Format Vega_ForceDirected -Path graph.html
```

## PARAMETERS

### -Format
Desired export format / layout pipeline (see Description for options).

```yaml
Type: GraphExportTypes
Parameter Sets: (All)
Aliases:
Accepted values: Graphviz, GraphML, MSAGL_MDS, MSAGL_SUGIYAMA, MSAGL_FASTINCREMENTAL, Vega_ForceDirected, Vega_AdjacencyMatrix, Vega_TreeLayout

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Graph
Graph to export.

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

### -Path
Optional destination file. Extension chooses some sub-format behaviors (e.g. .html vs .json for Vega).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Internal PowerShell progress preference (not typically used).

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

### System.String
Graph representation string when -Path is not specified; nothing when writing to file.
## NOTES

## RELATED LINKS
Add-Edge
Add-Vertex
