---
external help file: PSGraph.dll-Help.xml
online version: 
schema: 2.0.0
---

# Add-Edge

## SYNOPSIS
Adds an edge between two vertexes into a graph

## SYNTAX

```
Add-Edge -From <Object> -To <Object> -Graph <Object> [-Attribute <Object>] [<CommonParameters>]
```

## DESCRIPTION
Adds an edge between two vertexes into a graph

## EXAMPLES

### Example 1
In this example new graph is created and stored in $g variable. Next line adds an edge from A to B into it. Vertexes A and B are automatically added to the graph. If vertexes are already in the graph they are used as source and target vertexes. In order for this to work vertex types has to be comparable.

```powershell code
PS C:\> $g = New-Graph -Type AdjacencyGraph
PS C:\> Add-Edge -From A -To B -Graph $g
```

## PARAMETERS

### -Attribute
Not used in current implementation

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

### -From
Source vertex to use for the edge

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

### -Graph
Graph to add vertexes and edges to

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

### -To
Target vertex to use for the edge

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
When addin edges library checks to see if there are such edges and vertices. If they are they are not added to the graph. Instead existing once are used.

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

