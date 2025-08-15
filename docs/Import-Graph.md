---
external help file: PSGraph.dll-Help.xml
Module Name: PSGraph
online version:
schema: 2.0.0
---

# Import-Graph

## SYNOPSIS
Import a GraphML file into a new PsBidirectionalGraph.

## SYNTAX

```
Import-Graph -Path <String> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Creates a new empty PsBidirectionalGraph and populates it by deserializing the specified GraphML
file (nodes and directed edges). Vertex labels become PSVertex ids. Edge tags are initialized with
empty PSEdgeTag instances. Only GraphML is supported; provide a valid path to a .graphml file.

## EXAMPLES

### Example 1
Load a previously exported GraphML file.
```powershell
$g = Import-Graph -Path ./graph.graphml
$g.VertexCount
$g.EdgeCount
```

## PARAMETERS

### -Path
Path to a GraphML (.graphml) file to deserialize.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### PSGraph.Model.PsBidirectionalGraph
The imported graph.
## NOTES

## RELATED LINKS
