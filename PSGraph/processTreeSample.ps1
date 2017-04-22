$ps = gwmi win32_process

class process : Psgraph.PSGraphVertex {
    [string]$ProcessName
    [int]$ProcessID
    [int]$ParentProcessId
    [string]get_UniqueKey() { return  $this.ProcessID }
}

$g = New-Graph -Type AdjacencyGraph

$ps | % {
    $p = [process]@{
        ProcessName = $_.ProcessName
        ProcessID = $_.ProcessID
        ParentProcessId = $_.ParentProcessId
        Label = $_.ProcessName
    }
    Add-Vertex -Vertex $p -Graph $g
}

$g.Vertices | % {
    $v1 = $_
    $v1.processname
    $v1.ProcessId
    $g.Vertices | % {
        $v2 = $_
        if ($v1.ProcessId -eq $v2.ParentProcessId){
            Add-Edge -From $v2 -To $v1 -Graph $g
        }
    }
}

Export-Graph -Graph $g -Format Graphviz -Path c:\temp\ps.gv


#Export graph
$graphFile = "c:\temp\ps.gv"
$svgOutFile = "c:\temp\ps.svg"
$pngOutFile = "c:\temp\ps.png"

Export-Graph -Graph $g -Format Graphviz -Path $graphFile
$tempFile = Get-Content $graphFile
$tempFile[0] += "`r`n" + "rankdir = LR"
$tempFile | Out-File $graphFile -Encoding ascii

pushd
cd c:\temp\graphviz\release\bin
.\dot.exe -Tsvg $graphFile -o $svgOutFile
.\dot.exe -Tpng $graphFile -o $pngOutFile
popd