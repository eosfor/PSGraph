---
external help file: PSGraph.dll-Help.xml
online version: 
schema: 2.0.0
---

# Add-Vertex

## SYNOPSIS
Command takes an object and adds it as a vertex to a graph

## SYNTAX

```
Add-Vertex -Vertex <Object> -Graph <Object> [<CommonParameters>]
```

## DESCRIPTION
Command takes an object and adds it as a vertex to a graph. You must specify the graph to add the object to.

## EXAMPLES

### Example 1
```powershell code
$ps = gwmi win32_process

class process : Psgraph.PSGraphVertex {
    [string]$ProcessName
    [int]$ProcessID
    [int]$ParentProcessId
    [string]get_UniqueKey() { return  $this.ProcessID }
}

$g = New-Graph -Type AdjacencyGraph

$ps | % {
    $p = [process]@{
        ProcessName = $_.ProcessName
        ProcessID = $_.ProcessID
        ParentProcessId = $_.ParentProcessId
        Label = $_.ProcessName
    }
    Add-Vertex -Vertex $p -Graph $g
}
```
In this example $ps variable contains a list of processes.
This list is then converted into a set of  Psgraph.PSGraphVertex objects and each of these objects then added to a graph $g as a vertex

### Example 2
```powershell code
class process : Psgraph.PSGraphVertex {
    [string]$ProcessName
    [int]$ProcessID
    [int]$ParentProcessId
    [string]get_UniqueKey() { return  $this.ProcessID }
}
```
In this example new graph class is derived from Psgraph.PSGraphVertex exported from the module. This type is used as a base type because it contains all neccessary metadata which is needed to export graph into the DOT language format. Exported file then can be used by graphviz utility to visualize the graph.

## PARAMETERS

### -Graph
Takes a variable containing a graph instance

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

### -Vertex
Parameter takes an object as a vertex of a graph

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES
When adding new vertecies, library checks to see if they are alreagy in the graph. In case they are a new vertex is not added. For the basic .NET types this type of comparison works by default. However if you use your custom type which stores some additional metadata you need to provide a special method to compare them, see Example 2 for reference.

## RELATED LINKS

