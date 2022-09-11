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
Start-DSMClustering -Dsm $d
Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.svg -Format SVG
Export-DSM -Dsm $d -Path $Env:TMPDIR/dsm.txt -Format TEXT

Export-DSM -Dsm $d.Power(2) -Path $Env:TMPDIR/dsm2.svg -Format SVG
Export-DSM -Dsm $d.Power(2) -Path $Env:TMPDIR/dsm2.txt -Format TEXT

Export-DSM -Dsm $d.Power(3) -Path $Env:TMPDIR/dsm3.svg -Format SVG
Export-DSM -Dsm $d.Power(3) -Path $Env:TMPDIR/dsm3.txt -Format TEXT

Export-DSM -Dsm $d.Power(4) -Path $Env:TMPDIR/dsm4.svg -Format SVG
Export-DSM -Dsm $d.Power(4) -Path $Env:TMPDIR/dsm4.txt -Format TEXT

Export-DSM -Dsm $d.Power(5) -Path $Env:TMPDIR/dsm5.svg -Format SVG
Export-DSM -Dsm $d.Power(5) -Path $Env:TMPDIR/dsm5.txt -Format TEXT

Export-DSM -Dsm $d.Power(6) -Path $Env:TMPDIR/dsm6.svg -Format SVG
Export-DSM -Dsm $d.Power(6) -Path $Env:TMPDIR/dsm6.txt -Format TEXT


dot -Tsvg $graphFile -o $env:TMPDIR/srcGraph.svg

open $env:TMPDIR/srcGraph.svg
open $Env:TMPDIR/dsm.svg
open $Env:TMPDIR/dsm.txt
open $Env:TMPDIR/dsm2.svg
open $Env:TMPDIR/dsm3.svg
open $Env:TMPDIR/dsm4.svg
open $Env:TMPDIR/dsm5.svg
open $Env:TMPDIR/dsm6.svg

open $Env:TMPDIR/dsm3.txt
open $Env:TMPDIR/dsm4.txt
open $Env:TMPDIR/dsm5.txt
open $Env:TMPDIR/dsm6.txt