#create a new graph
$g = New-Graph

#add vertices
# [char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

'A', 'B', 'C', 'D','G' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

#add edges
Add-Edge -From A -To C -Graph $g | Out-Null
Add-Edge -From A -To D -Graph $g | Out-Null

Add-Edge -From B -To G -Graph $g | Out-Null

Add-Edge -From C -to A -Graph $g | Out-Null
Add-Edge -From C -To B -Graph $g | Out-Null
Add-Edge -From C -To G -Graph $g | Out-Null

Add-Edge -From D -To B -Graph $g | Out-Null

Add-Edge -From G -To D -Graph $g | Out-Null

#tmpFileNames

$dotFileName = "testGraph1.gv"
$svgFileName = "testGraph1.svg"


#Export graph
if ($IsMacOS) {
    $graphFile = Join-Path -Path $env:TMPDIR -ChildPath $dotFileName
    $svgOutFile = Join-Path -Path $env:TMPDIR -ChildPath $svgFileName
}

if ($IsWindows) {
    $graphFile = Join-Path -Path $env:TMP -ChildPath $dotFileName
    $svgOutFile = Join-Path -Path $env:TMP -ChildPath $svgFileName
}

Export-Graph -Graph $g -Format Graphviz -Path $graphFile
Export-Graph -Graph $g -Format MSAGL_MDS -Path $svgOutFile

# $graphFile
# $svgOutFile

$d = New-DSM -Graph $g
# Start-DSMClustering -Dsm $d
Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.svg -Format SVG
Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.txt -Format TEXT

dot -Tsvg $graphFile -o $env:TMPDIR/srcGraph.svg