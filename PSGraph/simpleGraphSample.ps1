#create a new graph
$g = New-Graph -Type AdjacencyGraph

#add vertices
[char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

#add edges
Add-Edge -From A -To D -Graph $g | Out-Null
Add-Edge -From A -To E -Graph $g | Out-Null
Add-Edge -From D -to F -Graph $g | Out-Null
Add-Edge -From E -To F -Graph $g | Out-Null
Add-Edge -From E -To G -Graph $g | Out-Null
Add-Edge -From G -To M -Graph $g | Out-Null
Add-Edge -From B -To E -Graph $g | Out-Null
Add-Edge -From B -To G -Graph $g | Out-Null
Add-Edge -From B -To H -Graph $g | Out-Null
Add-Edge -From H -To I -Graph $g | Out-Null
Add-Edge -From I -To L -Graph $g | Out-Null
Add-Edge -From C -To J -Graph $g | Out-Null
Add-Edge -From J -To K -Graph $g | Out-Null
Add-Edge -From K -To L -Graph $g | Out-Null
Add-Edge -From O -To P -Graph $g | Out-Null


#Export graph
$graphFile = "c:\temp\testGraph1.gv"
$svgOutFile = "c:\temp\testGraph1.svg"
$pngOutFile = "c:\temp\testGraph1.png"

Export-Graph -Graph $g -Format Graphviz -Path $graphFile

pushd
cd c:\temp\graphviz\release\bin
.\dot.exe -Tsvg $graphFile -o $svgOutFile
.\dot.exe -Tpng $graphFile -o $pngOutFile
popd