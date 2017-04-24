---
external help file: PSGraph.dll-Help.xml
online version: 
schema: 2.0.0
---

# Export-Graph

## SYNOPSIS
Exports graph to an external format. Only DOT format is currently supported

## SYNTAX

```
Export-Graph -Graph <Object> -Format <ExportTypes> [-Path <String>] [-EdgeFormatter <Object>]
 [<CommonParameters>]
```

## DESCRIPTION
Exports graph to an external format. Only DOT format is currently supported

## EXAMPLES

### Example 1
Exports graph $g to a file $graphFile
```powershell code
PS C:\> Export-Graph -Graph $g -Format Graphviz -Path $graphFile
```

### Example 2
Exports the gaph $g to a dot file. If an edge leads to a vertex of ClassicCircuit type it becomes of a (200,215,0,44)RGBA color.
```powershell code
$fmt2 = {
    param($edge, $edgeFormatter)
    If ($edge.Target -is [ClassicCircuit]) {
            $edgeFormatter.StrokeGraphvizColor = [QuickGraph.Graphviz.Dot.GraphvizColor]::new(200,215,0,44)
    }
}

Export-Graph -Graph $g -Format Graphviz -Path $graphFile -EdgeFormatter $fmt2 -Verbose

```

## PARAMETERS

### -EdgeFormatter
A callback scriptblock used to customize layout of the graph edges

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format
{{Fill Format Description}}

```yaml
Type: ExportTypes
Parameter Sets: (All)
Aliases: 
Accepted values: Graphviz

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Graph
{{Fill Graph Description}}

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
{{Fill Path Description}}

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

