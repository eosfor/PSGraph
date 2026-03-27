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

$dsm1 = New-DSM -graph $g
$ret1 = Start-DSMClustering -Dsm $dsm1 -ClusteringAlgorithm Classic
# Visual rendering now lives in PSGraphView.
Export-DSMView -Result $ret1 -Renderer DsmVegaMatrix -As Html -Path $Env:TMPDIR/dsmPartitioned1.html
open $Env:TMPDIR/dsmPartitioned1.html


$dsm2 = New-DSM -graph $g
$ret2 = Start-DSMClustering -Dsm $dsm2 -ClusteringAlgorithm GraphBased
Export-DSMView -Result $ret2 -Renderer DsmVegaMatrix -As Html -Path $Env:TMPDIR/dsmPartitioned2.html
open $Env:TMPDIR/dsmPartitioned2.html


$ret3 = Start-DSMSequencing -Dsm $dsm2
Export-DSMView -SequencedDsm $ret3 -Renderer DsmVegaMatrix -As Html -Path $Env:TMPDIR/dsmPartitioned3.html
open $Env:TMPDIR/dsmPartitioned3.html


Export-GraphView -Graph $g -Renderer VegaForceDirected -As Html -Path $Env:TMPDIR/dsmPartitioned4.html
open $Env:TMPDIR/dsmPartitioned4.html