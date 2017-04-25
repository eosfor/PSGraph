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
Exports graph $g to a dot file $graphFile
```powershell code
PS C:\> Export-Graph -Graph $g -Format Graphviz -Path $graphFile
```

### Example 2
Exports the graph $g to a dot file. If an edge leads to a vertex of ClassicCircuit type it becomes of a (200,215,0,44)RGBA color.
```powershell code
$fmt2 = {
    param($edge, $edgeFormatter)
    If ($edge.Target -is [ClassicCircuit]) {
            $edgeFormatter.StrokeGraphvizColor = [QuickGraph.Graphviz.Dot.GraphvizColor]::new(200,215,0,44)
    }
}

Export-Graph -Graph $g -Format Graphviz -Path $graphFile -EdgeFormatter $fmt2 -Verbose
```

### Example 3
This example shows how to define a derived class and configure properties of the vertex. In this example the class ClassicCircuit is defined. It is derived from Psgraph.PSGraphVertex. Default constructor takes the necessary parameters and and sets Label and Shape properties. When exported these properties influence the visualization.
```powershell code
class ClassicCircuit : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceType
    [string]$SubscriptionID
    ClassicCircuit([string]$n, [string]$loc, [string]$sID){
        $this.name = $n
        $this.ResourceType = "ClassicMPLS"
        $this.SubscriptionID = $sID
        $this.Label = $n
        $this.Shape =  "Circle"
    }

    [string]get_UniqueKey() { return $this.Label }
}
```

## PARAMETERS

### -EdgeFormatter
A callback scriptblock used to customize layout of an edges of a graph

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
Format to use for export. Only Graphviz format is currently supported

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
A graph to be exported

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
Path to put the exported file

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
The export procedure is pretty complex. It relies on the export capabilities provided by QuickGraph library. When the graph is filled up with vertexes and edges it can be exported to a various formats. Most common format is a dot format which can be used by graphviz utility. 
If you use standard built-in .NET types for vertexes, and you don't need to customize the output of the graph with a different colors or layout options you can use this cmdlet as is, you don't need to put additional efforts. On the other hand if you need to add colors or change some other parameters of vertexes and edges some additional code is required.

Changing the layout options for vertexes
Layout configuration of vertexes is based on the class PSGraphVertex, exposed by this module. This class in turn is derived from GraphvizVertex from the original library QuickGraph. This class contains all necessary properties needed to manage layout of the edges. If you want to configure visual representation of the graph, you need to derive from PSGraphVertex and set properties you need, see Example 3.

Changing layout options for edges
In order to manage visualization of an edge you need to take a different approach. For this EdgeFormatter parameter is used. It takes a scriptblock, which is called each time an edge gets exported. You can put a logic into the scriptblock and so configure visualization of each edge, see Example 2

## RELATED LINKS

