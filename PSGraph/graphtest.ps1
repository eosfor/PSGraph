$g = New-Graph -Type AdjacencyGraph


class vnet : Psgraph.PSGraphVertex {
    [string]$iprange;
    [string]$type = "vnet"
}

class ER : Psgraph.PSGraphVertex {
    [string]$ipaddress;
    [string]$type = "ER"
}

Add-Edge -from ([vnet]@{Label = "aaa";  shape = "Parallelogram"; iprange = "192.168.1.0/24"}) -to ([ER]@{Label = "xxx";  ipaddress = "192.168.2.1"}) -Graph $g
Add-Edge -from ([vnet]@{Label = "bbb";  iprange = "192.168.2.0/24"}) -to ([ER]@{Label = "yyy";  ipaddress = "192.168.3.1"}) -Graph $g
Add-Edge -from ([vnet]@{Label = "ccc";  iprange = "192.168.2.0/24"}) -to ([ER]@{Label = "zzz";  ipaddress = "192.168.4.1"}) -Graph $g



Export-Graph -Graph $g -Format Graphviz -Path c:\tests\testgraph.txt


C:\Temp\graphviz\release\bin\dot.exe -Tpng "C:\tests\testgraph.txt" -o 'C:\tests\testgraph.png'