Get-EventSubscriber | Unregister-Event

$c = [QuickGraph.Graphviz.Dot.GraphvizColor]::new(200,215,0,44)

$edgeHandler = {
    "handler" | Out-File c:\temp\log.txt -Append
    $Event.SourceArgs.VertexFormatter.Color = $global:c
}


$vertexHandler = {
        $Event.SourceArgs.VertexFormatter.Fillcolor = $global:c
        $Event.SourceArgs.VertexFormatter.Style = "Filled"
}

$gva = New-GraphvizAlgorithm -Graph $g

Register-ObjectEvent -EventName FormatEdge -Action $edgeHandler -InputObject $gva

$k = $gva.Generate()