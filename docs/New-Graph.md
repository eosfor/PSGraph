---
external help file: PSGraph.dll-Help.xml
online version: 
schema: 2.0.0
---

# New-Graph

## SYNOPSIS
Command creates an instance onf a graph object.

## SYNTAX

```
New-Graph -Type <PsGraphType> [<CommonParameters>]
```

## DESCRIPTION
Command creates an instance onf a graph object. In order to do that it takse a graph type parameter, which specifies the type of the graph to create.

## EXAMPLES

### Example 1
```
PS C:\> New-Graph -Type AdjacencyGraph
```

Returns an instance of a graph of type AdjacencyGraph

## PARAMETERS

### -Type
The type of a graph. BidirectionalMatrixGraph is not supported

```yaml
Type: PsGraphType
Parameter Sets: (All)
Aliases: 
Accepted values: AdjacencyGraph, BidirectionalGraph, BidirectionalMatrixGraph, UndirectedGraph

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

## RELATED LINKS

