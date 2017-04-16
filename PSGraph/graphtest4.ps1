#region declaring classes
class VNET : Psgraph.PSGraphVertex {
    [string]$SubscriptionID
    [string]$Name
    [string]$ResourceType
    [string]$ResourceGroupName
    [string]$Location
    [array]$Subnets
    [string]$AddressSpace
    [string]$DHCPOptions
    [string]$PeeredNetwork
    [string]$VNETType = "ARM"
    [string]$ResourceID
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND ($this.Label -eq ([VNET]$y).Label) -AND ($this.ResourceID -eq ([VNET]$y).ResourceID)}
    [int]GetHashCode() {return $this.label.gethashcode()}
}

class Subscription : Psgraph.PSGraphVertex {
    [string] $SubscriptionID
    [string] $SubscriptionName
    [string] $ResourceID

    Subscription([string] $SubID, [string] $SubName){
        $this.SubscriptionID = $SubID
        $this.SubscriptionName = $SubName
        $this.Label = $SubName
        $this.Shape =  "Rectangle"
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND ($this.Label -eq ([Subscription]$y).Label)}
    [int]GetHashCode() {return $this.label.gethashcode()}
}

class Gateway : Psgraph.PSGraphVertex{
    [string]$Name
    [string]$ResourceType
    [string]$SubscriptionID
    [array] $Properties
    [string]$ResourceID
    Gateway($name, $rID, $rType, $sID,  $Prop){
        $this.Name = $name
        $this.ResourceID = $rID
        $this.ResourceType = $rType
        $this.SubscriptionID = $sID
        $this.Properties = $Prop
        $this.Label = $name
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND ($this.Label -eq ([Gateway]$y).Label) -AND ($this.ResourceID -eq ([Gateway]$y).ResourceID)}
    [int]GetHashCode() {return $this.Label.GetHashCode() -xor $this.ResourceID.GetHashCode()}
}

class Connection : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceName
    [string]$ResourceType
    [string]$Location
    [string]$SubscriptionID
    [array] $Properties
    [string]$ResourceID
    Connection([string]$n, [string]$rID, [string]$rName, [string]$rType, [string]$loc, [string]$sID, [array]$p, [string]$s = "Diamond"){
        $this.name = $n
        $this.ResourceID = $rID
        $this.Resourcename = $rName
        $this.ResourceType = $rType
        $this.Location = $loc
        $this.SubscriptionID = $sID
        $this.Properties = $p
        $this.Label = $n
        $this.Shape = $s
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND ($this.Label -eq ([Connection]$y).Label) -AND ($this.ResourceID -eq ([Connection]$y).ResourceID)}
    [int]GetHashCode() {return $this.Label.GetHashCode() -xor $this.ResourceID.GetHashCode()}
}

class Circuit : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceName
    [string]$ResourceType
    [string]$Location
    [string]$SubscriptionID
    [array] $Properties
    [string]$ResourceID
    Circuit([string]$n, [string]$rID, [string]$rName, [string]$rType, [string]$loc, [string]$sID, [array]$p){
        $this.name = $n
        $this.ResourceID = $rID
        $this.Resourcename = $rName
        $this.ResourceType = $rType
        $this.Location = $loc
        $this.SubscriptionID = $sID
        $this.Properties = $p
        $this.Label = $n
        $this.Shape =  "Circle"
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND ($this.Label -eq ([Circuit]$y).Label) -AND ($this.ResourceID -eq ([Circuit]$y).ResourceID)}
    [int]GetHashCode() {return $this.Label.GetHashCode() -xor $this.ResourceID.GetHashCode()}

}

class ClassicVNET : Psgraph.PSGraphVertex {
    [string]$SubscriptionID
    [string]$Name
    [string]$ResourceType
    [string]$ResourceGroupName
    [string]$Location
    [array]$Subnets
    [string]$AddressSpace
    [string]$DHCPOptions
    [string]$PeeredNetwork
    [string]$VNETType = "ARM"
    [string]$ResourceID
    [string]$MPLSGWConnection 
    [bool]Equals([System.Object]$y)  { return $this.IsTypeEqual($y) -AND  ($this.Label -eq ([ClassicVNET]$y).Label)}
    [int]GetHashCode() {return $this.label.gethashcode()}
    ClassicVNET($sID, $n, $rType, $loc, $sub, $as, $dhcp, $mpls){
            $this.Label = $n
            $this.SubscriptionId = $sID
            $this.Name  =  $n
            $this.ResourceType = "ClassicNetwork"
            $this.Location = $loc
            $this.Subnets = $sub
            $this.AddressSpace = $as
            $this.DHCPOptions = $dhcp
            $this.PeeredNetwork = "NA"
            $this.ResourceID = "NA"
            $this.MPLSGWConnection = $mpls
            $this.Shape = "DoubleCircle"
    }
}

class ClassicCircuit : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceType
    [string]$SubscriptionID
    ClassicCircuit([string]$n, [string]$loc, [string]$sID){
        $this.name = $n
        $this.ResourceType = "ClassicMPLS"
        $this.SubscriptionID = $sID
        $this.Label = $n
        $this.Shape =  "Circle"
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND  ($this.Label -eq ([ClassicCircuit]$y).Label)}
    [int]GetHashCode() {return $this.label.gethashcode()}
}
#endregion declaring classes

#region helper functions
function getAllVNETS($subscription) {
    [array]$ret = $null
    foreach ($sub in $subscription) {
        $ret += Get-AzureRmResource -ResourceId "https://management.azure.com/subscriptions/$($sub.SubscriptionId)/providers/Microsoft.Network/virtualNetworks"
    }

    $ret
}

function getAllGWs($subscription) {
    [array]$ret = $null
    foreach ($sub in $subscription) {
        $ret += Get-AzureRmResource -ResourceId "https://management.azure.com/subscriptions/$($sub.SubscriptionId)/providers/Microsoft.Network/virtualNetworkGateways"
    }

    $ret
}

function getAllConnections($subscription) {
    [array]$ret = $null
    foreach ($sub in $subscription) {
        $ret += Get-AzureRmResource -ResourceId "https://management.azure.com/subscriptions/$($sub.SubscriptionId)/providers/Microsoft.Network/connections"
    }

    $ret
}

#$subs  =  Get-AzureRmSubscription

#$allVnets = getAllVNETS -subscription $subs
#$allGWs  = getAllGWs -subscription $subs
#$allConections = getAllConnections -subscription $subs
#endregion helper functions


$g = New-Graph -Type AdjacencyGraph # -EnableVertexComparer

#region fill in the graph
#add vnets
foreach ($vnet in $allVnets) {
    $vnetHash = @{
        Label = $vnet.Name
        SubscriptionId = $vnet.SubscriptionId
        Name  =  $vnet.Name
        ResourceType = $vnet.ResourceType
        Location = $vnet.Location
        Subnets = $vnet.Properties.subnets
        AddressSpace = $vnet.Properties.addressSpace.addressPrefixes
        DHCPOptions = $vnet.Properties.dhcpOptions.dnsServers
        PeeredNetwork = $vnet.Properties.virtualNetworkPeerings.properties.remoteVirtualNetwork.id
        ResourceID = $vnet.ResourceId
    }
    Add-Vertex -Vertex([VNET]$vnetHash) -Graph $g
}

#add subscriptions
foreach ($s  in $subs){
    $subObj = [Subscription]::new($s.SubscriptionId, $s.SubscriptionName)
    Add-Vertex -Vertex $subObj -Graph $g
}

#add gateways
foreach ($gw in $allGWs) {
    $gwObject = [Gateway]::New($gw.name, $gw.resourceid, $gw.Resourcetype, $gw.subscriptionid, $gw.properties)
    Add-Vertex -Vertex $gwObject -Graph $g
}

#add connections
foreach ($conn in $allConections) {
    if ($conn.Properties.connectionType   -eq "ExpressRoute") {
        $connObject = [Connection]::new($conn.Name, $conn.ResourceID, $conn.Resourcename, $conn.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties, "Diamond")
    }
    elseif ($conn.Properties.connectionType   -eq "IPsec") {
        $connObject = [Connection]::new($conn.Name, $conn.ResourceID, $conn.Resourcename, $conn.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties, "Parallelogram")
    } elseif ($conn.Properties.connectionType   -eq "Vnet2Vnet") {
        $connObject = [Connection]::new($conn.Name, $conn.ResourceID, $conn.Resourcename, $conn.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties, "Box")
    }
    Add-Vertex -Vertex $connObject -Graph $g
}

#add MPLS
foreach ($er in $allCircuits) {
    $circuitObject = [Circuit]::new($er.Name, $er.ResourceID, $er.Resourcename, $er.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties)
    Add-Vertex -Vertex $circuitObject -Graph $g
}

#add classic VNETS
foreach ($vnet in $classicVnets) {
    $vnetSite = $vnet.Netconfig.NetworkConfigXML.NetworkConfiguration.VirtualNetworkConfiguration.VirtualNetworkSites.VirtualNetworkSite
    if ($vnetSite) {
        $vnetSite | % {
            $gwConnection = $vnetSite.Gateway.ConnectionsToLocalNetwork.LocalNetworkSiteRef
            if ($gwConnection) {
                $mplsConnection =
                    $gwConnection | select -ExpandProperty Name
            }

            $vnetHash = [ClassicVNET]::new( $vnet.SubscriptionId,
                                            $_.Name,
                                            "ClassicNetwork",
                                            $_.Location,
                                            $_.subnets,
                                            $_.addressSpace.addressPrefixes,
                                            $_.Dns.DnsServers.DnsServer,
                                            $mplsConnection)
            Add-Vertex -Vertex $vnetHash -Graph $g
        }
    }
}


#add classic circuits
foreach ($er in $classicVnets) {
    $vnetSite = $er.Netconfig.NetworkConfigXML.NetworkConfiguration.VirtualNetworkConfiguration.VirtualNetworkSites.VirtualNetworkSite
    if ($vnetSite) {
        $gwConnection = $vnetSite.Gateway.ConnectionsToLocalNetwork.LocalNetworkSiteRef
        if ($gwConnection) {
            $gwConnection | % {
                $circuitObject = [ClassicCircuit]::new($_.Name, $er.Location, $er.SubscriptionId)
                Add-Vertex -Vertex $circuitObject -Graph $g
                #$g.AddVertex($circuitObject)
            }
        }
    }
}

#endregion fill in the graph

#region build default indexes
# prepare indexes
$vnetsByGW = @{}
$connByGW = @{}
$connByER = @{}
$vnetBySubscription = @{}

#vnet by subscription
foreach ($element in ($g.Vertices  | ? {($_ -is [VNET]) -or ($_ -is [ClassicVNET])})){
    $vnetBySubscription[$element.SubscriptionID] = $vnetBySubscription[$element.SubscriptionID]  + (,$element)
}

#circuits  by  ID
foreach ($element in ($g.Vertices  | ? {$_ -is [Circuit]})){
    if ($element.resourceid) {
        $element | % {$connByER[$_.resourceid] = $connByER[$_]  + (,$element)}
    }
    else {$connByER['no Express  Route'] = $connByER['no Express  Route'] + (,$element)}
}

#conn by GW link
foreach ($element in ($g.Vertices  | ? {$_ -is [Connection]})){
    if ($element.properties.virtualNetworkGateway1.id) {
        $element | % {$connByGW[$_.properties.virtualNetworkGateway1.id] = $connByGW[$_]  + (,$element)}
    }
    else {$connByGW['no connection object'] = $connByGW['no connection object'] + (,$element)}
}

#vnet by GW link
foreach ($element in ($g.Vertices  | ? {$_ -is [VNET]})){
    $gwSubnet = $element.Subnets | ?  name -eq "GatewaySubnet"
    if ($gwSubnet.Properties.IpConfigurations.ID) {
        $gwSubnet.Properties.IpConfigurations.ID | % {$vnetsByGW[$_] = $vnetsByGW[$_]  + (,$element)}
    }
    else {$vnetsByGW['no external connection'] = $vnetsByGW['no external connection'] + (,$element)}
}
#endregion build default indexes

#region adding edges
#VNET -> VNET peerings
foreach ($v in ($g.Vertices  | ? {$_ -is [VNET]})){
    foreach ($v2 in ($g.Vertices  | ? {$_ -is [VNET]})){
        if ($v.ResourceID -eq $v2.PeeredNetwork) {
            Add-Edge -From $v -To $v2 -Graph $g
        }        
    }    
}

#Subscriptions -> VNET
foreach ($v in ($g.Vertices  | ? {$_ -is [Subscription]})){
    $v2 = $vnetBySubscription[$v.SubscriptionID]
    if ($v2) {
        $v2 | % {Add-Edge -From $v -To $_ -Graph $g}
    }
}

#VNET -> VNET GW
foreach ($v in ($g.Vertices  | ? {$_ -is [Gateway]})){
    $vnetToConnect = $vnetsByGW[$v.Properties.IpConfigurations.ID]
    if ($vnetToConnect){
        $vnetToConnect | % {Add-Edge -From $_ -To $v -Graph $g}
    }
}

#VNET GW -> GW Connection
foreach ($v in ($g.Vertices  | ? {$_ -is [Gateway]})) {
    $connectionToConnect = $connByGW[$v.ResourceID]
    if ($connectionToConnect){
        $connectionToConnect | % {Add-Edge -From $v -To $_ -Graph $g}
    }
}

#GW Connection -> MPLS Link
foreach ($element in ($g.Vertices  | ? {$_ -is [Connection]})){
    if ($element.Properties.peer.id) {
        $mpls = $connByER[$element.Properties.peer.id]
        if ($mpls) {
            $mpls | % {Add-Edge -From $element -To $_ -Graph $g}
        }
    }
}

foreach ($cv in  ($g.vertices  | where {$_ -is [ClassicVNET]})){
    foreach ($ce in ($g.vertices  | where {$_ -is [ClassicCircuit]})){
        if ($cv.MPLSGWConnection -eq $ce.Name) {
            add-edge -from $cv -to $ce -graph $g
        }
    }
}

#endregion adding edges


#Export graph
$graphFile = "c:\temp\testGraph.gv"
$svgOutFile = "c:\temp\testGraph.svg"
$pngOutFile = "c:\temp\testGraph.png"

Export-Graph -Graph $g -Format Graphviz -Path $graphFile  -Verbose
$tempFile = Get-Content $graphFile
$tempFile[0] += "`r`n" + "rankdir = LR"
$tempFile | Out-File $graphFile -Encoding ascii

pushd
cd c:\temp\graphviz\release\bin
.\dot.exe -Tsvg $graphFile -o $svgOutFile
.\dot.exe -Tpng $graphFile -o $pngOutFile
popd