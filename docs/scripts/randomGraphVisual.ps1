$nodeCount = 50
$edgeCount = 300

$v = [PSGraph.Model.PSVertex[]]::new($nodeCount)
$g = New-Graph

# вершины
0..($nodeCount-1) | ForEach-Object {
    $v[$_] = [PSGraph.Model.PSVertex]::new($_.ToString())
    Add-Vertex -Vertex $v[$_] -Graph $g
}

# рёбра без дублей
$edgeSet = @{}
while ($edgeSet.Count -lt $edgeCount) {
    $s = Get-Random -Maximum $nodeCount        # 0..9
    $t = Get-Random -Maximum $nodeCount
    if ($s -eq $t) { continue }

    $key = "$s->$t"
    if (-not $edgeSet.ContainsKey($key)) {
        Add-Edge -From $v[$s] -To $v[$t] -Graph $g
        $edgeSet[$key] = $true
    }
}

"Graph ready: $($g.VertexCount) vertices, $($g.EdgeCount) edges."

$dsm = New-DSM -graph $g
$ret = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm Classic
Export-DSM -Result $ret -Format VEGA_HTML -Path $Env:TMPDIR/dsmPartitioned1.html
open $Env:TMPDIR/dsmPartitioned1.html