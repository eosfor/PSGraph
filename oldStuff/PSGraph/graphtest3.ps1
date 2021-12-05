$subs = Import-Clixml "C:\tests\objects\subs.xml" #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allVnets =  Import-Clixml -Path C:\tests\objects\vnets.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allGWs =  Import-Clixml -Path C:\tests\objects\gws.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allConections =  Import-Clixml -Path C:\tests\objects\connections.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allCircuits =  Import-Clixml -Path C:\tests\objects\circuits.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$classicVnets = Import-Clixml "C:\tests\objects\classicNets.xml"

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
}

class Subscription : Psgraph.PSGraphVertex {
    [string] $SubscriptionID
    [string] $SubscriptionName

    Subscription([string] $SubID, [string] $SubName){
        $this.SubscriptionID = $SubID
        $this.SubscriptionName = $SubName
        $this.Label = $SubName
        $this.Shape =  "Rectangle"
    }
}

class Gateway : Psgraph.PSGraphVertex{
    [string]$Name
    [string]$ResourceType
    [string]$SubscriptionID
    [array] $Properties
    Gateway($name, $rID, $rType, $sID,  $Prop){
        $this.Name = $name
        $this.ResourceID = $rID
        $this.ResourceType = $rType
        $this.SubscriptionID = $sID
        $this.Properties = $Prop
        $this.Label = $name
    }
}

class Connection : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceName
    [string]$ResourceType
    [string]$Location
    [string]$SubscriptionID
    [array] $Properties
    Connection([string]$n, [string]$rID, [string]$rName, [string]$rType, [string]$loc, [string]$sID, [array]$p){
        $this.name = $n
        $this.ResourceID = $rID
        $this.Resourcename = $rName
        $this.ResourceType = $rType
        $this.Location = $loc
        $this.SubscriptionID = $sID
        $this.Properties = $p
        $this.Label = $n
        $this.Shape = "Diamond"
    }

}

class Circuit : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceName
    [string]$ResourceType
    [string]$Location
    [string]$SubscriptionID
    [array] $Properties
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

$g = New-Graph -Type BidirectionalGraph -EnableVertexComparer

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
    $connObject = [Connection]::new($conn.Name, $conn.ResourceID, $conn.Resourcename, $conn.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties)
    Add-Vertex -Vertex $connObject -Graph $g
}

#add MPLS
foreach ($er in $allCircuits) {
    $circuitObject = [Circuit]::new($er.Name, $er.ResourceID, $er.Resourcename, $er.ResourceType, $conn.Location, $conn.SubscriptionID, $conn.Properties)
    Add-Vertex -Vertex $circuitObject -Graph $g
}
#endregion fill in the graph

#region build default indexes
# prepare indexes
$vnetsByGW = @{}
$connByGW = @{}
$connByER = @{}
$vnetBySubscription = @{}

#vnet by subscription
foreach ($element in ($g.Vertices  | ? {$_ -is [VNET]})){
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