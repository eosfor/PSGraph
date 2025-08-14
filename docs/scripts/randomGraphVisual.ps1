$nodeCount = 50
$edgeCount = 100

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

$dsm1 = New-DSM -graph $sg
$ret1 = Start-DSMClustering -Dsm $dsm1 -ClusteringAlgorithm Classic
Export-DSM -Result $ret1 -Format VEGA_HTML -Path $Env:TMPDIR/dsmPartitioned1.html
open $Env:TMPDIR/dsmPartitioned1.html


$dsm2 = New-DSM -graph $sg
$ret2 = Start-DSMClustering -Dsm $dsm2 -ClusteringAlgorithm Graph
Export-DSM -Result $ret2 -Format VEGA_HTML -Path $Env:TMPDIR/dsmPartitioned2.html
open $Env:TMPDIR/dsmPartitioned2.html


$ret3 = Start-DSMSequencing -Dsm $dsm2
Export-DSM -SequencedDsm $ret3 -Format VEGA_HTML -Path $Env:TMPDIR/dsmPartitioned3.html
open $Env:TMPDIR/dsmPartitioned3.html


Export-Graph -Graph $g -Format Vega_ForceDirected -Path $Env:TMPDIR/dsmPartitioned4.html
open $Env:TMPDIR/dsmPartitioned4.html