$g = New-Graph -Type AdjacencyGraph


class vnet : Psgraph.PSGraphVertex {
    [string]$iprange;
    [string]$type = "vnet"
}

class ER : Psgraph.PSGraphVertex {
    [string]$ipaddress;
    [string]$type = "ER"
}

[ER]$er  = @{Label = "yyy";  ipaddress = "192.168.3.1"}
[ER]$erEnd  = @{Label = "end";  ipaddress = "192.168.10.1"}

Add-Edge -from ([vnet]@{Label = "aaa";  shape = "Parallelogram"; iprange = "192.168.1.0/24"}) -to $er -Graph $g
Add-Edge -from ([vnet]@{Label = "bbb";  iprange = "192.168.2.0/24"}) -to $er -Graph $g
Add-Edge -from $er -to $erEnd -Graph $g
Add-Edge -from ([vnet]@{Label = "ccc";  iprange = "192.168.2.0/24"}) -to ([ER]@{Label = "zzz";  ipaddress = "192.168.4.1"}) -Graph $g


$e = $g.vertices | where iprange -EQ "192.168.1.0/24"
$e1 = $g.vertices | where ipaddress -EQ "192.168.10.1"

get-graphpath -from $e -to $e1 -graph $g

Export-Graph -Graph $g -Format Graphviz -Path c:\tests\testgraph.txt




C:\Temp\graphviz\release\bin\dot.exe -Tpng "C:\tests\testgraph.txt" -o 'C:\tests\testgraph.png'