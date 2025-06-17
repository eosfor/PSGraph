#create a new graph
$g = New-Graph

#add vertices
# [char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

'A', 'D', 'E', 'F','G','M','B','H','I' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

#add edges
Add-Edge -From A -To D -Graph $g | Out-Null
Add-Edge -From A -To E -Graph $g | Out-Null
Add-Edge -From D -to F -Graph $g | Out-Null
Add-Edge -From E -To F -Graph $g | Out-Null
# Add-Edge -From E -To G -Graph $g | Out-Null
Add-Edge -From G -To M -Graph $g | Out-Null
Add-Edge -From B -To E -Graph $g | Out-Null
Add-Edge -From B -To G -Graph $g | Out-Null
Add-Edge -From B -To H -Graph $g | Out-Null
Add-Edge -From H -To I -Graph $g | Out-Null
Add-Edge -From M -To B -Graph $g | Out-Null

#Add-Edge -From I -To L -Graph $g | Out-Null
#Add-Edge -From C -To J -Graph $g | Out-Null
#Add-Edge -From J -To K -Graph $g | Out-Null
#Add-Edge -From K -To L -Graph $g | Out-Null
#Add-Edge -From O -To P -Graph $g | Out-Null

#Add-Edge -From F -to B -Graph $g | Out-Null
#Add-Edge -From P -To O -Graph $g | Out-Null
#Add-Edge -From K -To C -Graph $g | Out-Null

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
$ret = Start-DSMClustering -Dsm $d

Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.svg -Format SVG
Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.txt -Format TEXT

Export-DSM -Result $ret -Path $Env:TMPDIR/dsmPartitioned.svg

dot -Tsvg $graphFile -o $env:TMPDIR/srcGraph.svg