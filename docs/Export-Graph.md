---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Export-Graph

## SYNOPSIS
Export a graph to Graphviz DOT or GraphML.

## SYNTAX

```
Export-Graph -Graph <PsBidirectionalGraph> -Format <GraphExportTypes> [-Path <String>]
 [-GraphScript <ScriptBlock>] [-VertexScript <ScriptBlock>] [-EdgeScript <ScriptBlock>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Exports a PsBidirectionalGraph to one of the supported textual/interchange formats:

* Graphviz – DOT text (layout performed later by graphviz tools). This stays native to `PSGraph`.
* GraphML – XML interchange format. This also stays native to `PSGraph` and is the recommended neutral import/export format.

If -Path is supplied the content is written to that file; otherwise the string is written to the pipeline.

Visual rendering is no longer provided by `PSGraph`. Use `PSGraphView` and `Export-GraphView` for Vega or MSAGL output.

## EXAMPLES

### Example 1
Export to Graphviz DOT (string to pipeline).
```powershell
$g = New-Graph
Add-Edge -From A -To B -Graph $g
Export-Graph -Graph $g -Format Graphviz | Out-File graph.dot
```

### Example 2
Round-trip a GraphML file.
```powershell
$g = New-Graph; Add-Edge -From A -To B -Graph $g; Add-Edge -From B -To C -Graph $g
Export-Graph -Graph $g -Format GraphML -Path graph.graphml
```

### Example 3
Export domain objects to Graphviz DOT with per-vertex attributes.
```powershell
$g = New-Graph

$a = [pscustomobject]@{ type = 'value'; Name = 'a'; data = 2.0; grad = 6.0 }
$b = [pscustomobject]@{ type = 'value'; Name = 'b'; data = -3.0; grad = -4.0 }
$mul = [pscustomobject]@{ type = 'operation'; Name = '*' }
$e = [pscustomobject]@{ type = 'value'; Name = 'e'; data = -6.0; grad = -2.0 }

Add-Edge -Graph $g -From $a -To $mul | Out-Null
Add-Edge -Graph $g -From $b -To $mul | Out-Null
Add-Edge -Graph $g -From $mul -To $e | Out-Null

$dot = Export-Graph -Graph $g -Format Graphviz `
    -GraphScript { @{ rankdir = 'LR'; label = 'Micrograd' } } `
    -VertexScript {
        $item = $_

        switch ($item.type) {
            'value' {
                @{
                    label = "{ $($item.Name) | data $('{0:F4}' -f $item.data) | grad $('{0:F4}' -f $item.grad) }"
                    shape = 'record'
                }
            }
            'operation' {
                @{
                    label = $item.Name
                    shape = 'ellipse'
                }
            }
        }
    }

$dot
```

The script blocks return Graphviz attributes as a hashtable, dictionary, or `PSCustomObject`. When vertices were created from PowerShell objects, `$_` is the original object. Named variables such as `$InputObject`, `$Object`, `$Vertex`, `$Edge`, `$Source`, `$Target`, and `$Graph` are also available where applicable.

Inside a PowerShell `switch` body, `$_` becomes the current switch value. Capture the original object first, for example `$item = $_`, when the script needs to keep using it.

## PARAMETERS

### -Format
Desired export format (see Description for options).

```yaml
Type: GraphExportTypes
Parameter Sets: (All)
Aliases:
Accepted values: Graphviz, GraphML

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

### -GraphScript
Optional script block that returns Graphviz graph attributes.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Optional destination file.

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

### -VertexScript
Optional script block invoked for each vertex. The script block should return Graphviz vertex attributes.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EdgeScript
Optional script block invoked for each edge. The script block should return Graphviz edge attributes.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

Graphviz / GraphML note:

* `Graphviz` is the textual DOT export that remains owned by `PSGraph`.
* `GraphML` is the interchange format that remains owned by `PSGraph` and can be round-tripped with `Import-Graph`.

For rendered output, use `PSGraphView` and `Export-GraphView`.

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
